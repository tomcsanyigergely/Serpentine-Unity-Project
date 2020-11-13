using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New ServerConfiguration", menuName = "Server Configuration")]
public class ServerConfiguration : ScriptableObject
{
    public UInt16 listenerPort;
    public UInt16 defaultServerPort;
    public string defaultHostName;
    public uint defaultNumberOfPlayers;
    public uint defaultLaps;
    public string lanDiscoveryRequest;
    public string lanDiscoveryResponse;
}
