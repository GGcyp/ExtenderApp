namespace ExtenderApp.Common.NetWorks
{
    public enum HeartbeatType
    {
        Ping = 0x0001,
        Pong = 0x0002,
        Emergency = 0x000F, // 紧急心跳
        Timeout,
    }
}
