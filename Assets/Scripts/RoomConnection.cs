using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class RoomConnection : MonoBehaviour
{
    public Button[] Rooms;
    private string roomName;

    public string RoomName { get => roomName; private set => roomName = value; }
    [DllImport("__Internal")]
    public static extern void EnterRoom(string roomID);
    // Start is called before the first frame update
    void Start()
    {
        foreach (var room in Rooms)
        {
            room.onClick.AddListener(() =>
            {
                RoomName = room.name;
                EnterRoom(RoomName);
            });
        }
    }
    private void Update()
    {
        foreach (var room in Rooms)
        {
            room.gameObject.SetActive(!NetworkManager.singleton.isNetworkActive);
        }
    }
}
