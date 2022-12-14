//dataプロトコル
//index = 0: 0の時はConnection要求，1の時はconnectionID受け渡し，2の時はdisconnect，３の時はData受け渡し(2バイト目はconnectionID)
//それ以降: Data(初めのconnectionID受け渡し時はDataなし)
public struct WebRTCMessageType
{
    public const byte ConnectionRequest = 0x00;
    public const byte ConnectionID = 0x01;
    public const byte Disconnect = 0x02;
    public const byte Data = 0x03;

}
