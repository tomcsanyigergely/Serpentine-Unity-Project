using System;

public class ServerSession
{
    public UInt16 serverPort;
    public string hostName;
    public uint numberOfPlayers;
    public uint laps;
}

public class ClientSession
{
    public string remoteServerIpAddress;
    public UInt16 remoteServerPort;
}

public static class GameSession
{
    public static ServerSession serverSession = null;
    public static ClientSession clientSession = null;
}
