using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class SoraTransport : Transport
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void Send_Data(byte[] data, int length);
    [DllImport("__Internal")]
    public static extern void ReInit();
    [DllImport("__Internal")]
    public static extern void InitJavaScriptSharedArray(byte[] byteOffset, int length);
     [DllImport("__Internal")]
    public static extern void ExitRoom();
#else
    void Send_Data(byte[] data, int length) { }
    void InitJavaScriptSharedArray(byte[] byteOffset, int length) { }
    void ExitRoom() { }
#endif
    bool serverActive = false;
    bool clientActive = false;
    int clientConnectionID = -1;
    private int maxPacketSize = 100;
    byte[] sharedBuffer;
    private void Start()
    {
        sharedBuffer = new byte[maxPacketSize * 8];
        InitJavaScriptSharedArray(sharedBuffer, sharedBuffer.Length);
    }

    public override bool Available()
    {
        return true;
    }
    public void ClientDataReceived(int length)
    {
        if (sharedBuffer[0] == WebRTCMessageType.ConnectionID && clientConnectionID == -1)
        {
            clientConnectionID = sharedBuffer[1];
            StartCoroutine(nextFrame());
            return;
        }
        if (sharedBuffer[0] == WebRTCMessageType.Disconnect && sharedBuffer[1] == clientConnectionID)
        {
            ClientDisconnect();
            return;
        }
        if (sharedBuffer[0] == WebRTCMessageType.Data && sharedBuffer[1] == clientConnectionID)
        {
            var segmentData = new ArraySegment<byte>(sharedBuffer, 2, length - 2);
            OnClientDataReceived.Invoke(segmentData, Channels.Reliable);
        }

    }
    IEnumerator nextFrame()
    {
        yield return null;
        OnClientConnected.Invoke();
        clientActive = true;
    }
    public void ServerDataReceived(int length)
    {
        if (sharedBuffer[0] == WebRTCMessageType.ConnectionRequest)
        {
            SendConnectionID();
            return;
        }
        if (sharedBuffer[0] == WebRTCMessageType.Disconnect)
        {
            OnServerDisconnected.Invoke(sharedBuffer[1]);
            return;
        }
        if (sharedBuffer[0] == WebRTCMessageType.Data)
        {
            var segmentData = new ArraySegment<byte>(sharedBuffer, 2, length - 2);
            OnServerDataReceived.Invoke(sharedBuffer[1], segmentData, Channels.Reliable);
        }


    }
    public override void ClientConnect(string address)
    {
        Debug.Log("Send Req");
        Send_Data(new byte[1] { WebRTCMessageType.ConnectionRequest }, 1);
    }
    public override bool ClientConnected()
    {
        return clientActive;
    }

    public override void ClientDisconnect()
    {
        if (!clientActive) { return; }
        var dissconnectMessage = new byte[2] { WebRTCMessageType.Disconnect, (byte)clientConnectionID };
        Send_Data(dissconnectMessage, 2);
        clientActive = false;
        OnClientDisconnected.Invoke();
        ExitRoom();
    }

    public override void ClientSend(ArraySegment<byte> segment, int channelId = 0)
    {
        byte[] data = new byte[segment.Count + 2];
        Array.Copy(segment.Array, segment.Offset, data, 2, segment.Count);
        data[0] = WebRTCMessageType.Data;
        data[1] = (byte)clientConnectionID;
        Send_Data(data, data.Length);
    }

    public override int GetMaxPacketSize(int channelId = 0)
    {
        return 8 * maxPacketSize - 3;
    }

    public override bool ServerActive()
    {
        return serverActive;
    }

    public override void ServerDisconnect(int connectionId)
    {
        Send_Data(new byte[2] { WebRTCMessageType.Disconnect, (byte)connectionId }, 2);
    }

    public override string ServerGetClientAddress(int connectionId)
    {
        return "";
    }

    public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0)
    {
        byte[] data = new byte[segment.Count + 2];
        Array.Copy(segment.Array, segment.Offset, data, 2, segment.Count);
        data[0] = WebRTCMessageType.Data;
        data[1] = (byte)connectionId;
        Send_Data(data, data.Length);
    }

    public override void ServerStart()
    {
        Debug.Log("Server Start");
        serverActive = true;
    }

    public override void ServerStop()
    {
        Debug.Log("Server Stop");
        ExitRoom();
        serverActive = false;
    }

    public override Uri ServerUri()
    {
        return new Uri("localhost");
    }

    public override void Shutdown()
    {
        Debug.Log("Server Shutdown");
        serverActive = false;
    }
    public void SendConnectionID()
    {
        Debug.Log("Send Conn");
        var connectionID = NextConnectionId();
        Send_Data(new byte[2] { WebRTCMessageType.ConnectionID, connectionID }, 2);
        OnServerConnected.Invoke(connectionID);
    }

    int counter;
    public byte NextConnectionId()
    {
        int id = Interlocked.Increment(ref counter);

        // it's very unlikely that we reach the uint limit of 2 billion.
        // even with 1 new connection per second, this would take 68 years.
        // -> but if it happens, then we should throw an exception because
        //    the caller probably should stop accepting clients.
        // -> it's hardly worth using 'bool Next(out id)' for that case
        //    because it's just so unlikely.
        if (id == 0xff)
        {
            throw new Exception("connection id limit reached: " + id);
        }

        return (byte)id;
    }
}
