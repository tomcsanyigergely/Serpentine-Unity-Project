using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.Text;
using Unity.Entities;
using UnityEngine.Serialization;

public class NetworkListener : MonoBehaviour
{
    [SerializeField] private ServerConfiguration serverConfiguration;

    private UdpClient listener;

    private GoInGameServerSystem goInGameServerSystem;
    
    // Start is called before the first frame update
    private void Start()
    {
        listener = new UdpClient(serverConfiguration.listenerPort);
        foreach (var world in World.All)
        {
            var goInGameServerSystem = world.GetExistingSystem<GoInGameServerSystem>();
            if (goInGameServerSystem != null)
            {
                this.goInGameServerSystem = goInGameServerSystem;
                break;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (listener.Available > 0)
        {
            IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            string receivedMessage = Encoding.ASCII.GetString(listener.Receive(ref remoteEndpoint));
            
            Debug.Log("Received " + receivedMessage + " from address: " + remoteEndpoint.Address);
            
            byte[] response = Encoding.ASCII.GetBytes(serverConfiguration.lanDiscoveryResponse+ " " + GameSession.serverSession.serverPort + " " + goInGameServerSystem.connectedPlayers + " " + GameSession.serverSession.numberOfPlayers + " " + GameSession.serverSession.laps + " " + GameSession.serverSession.hostName);

            if (goInGameServerSystem.connectedPlayers < GameSession.serverSession.numberOfPlayers && receivedMessage.Equals(serverConfiguration.lanDiscoveryRequest))
            {
                listener.Send(response, response.Length, remoteEndpoint);
            }
        }
    }

    private void OnDestroy()
    {
        listener.Close();
    }
}
