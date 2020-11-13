using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using UnityEngine;

// Control system updating in the default world
[UpdateInWorld(UpdateInWorld.TargetWorld.ClientAndServer)]
public class Game : ComponentSystem
{
    // Singleton component to trigger connections once from a control system
    private struct UpdateOnce : IComponentData
    {
    }
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<UpdateOnce>();
        // Create singleton, require singleton for update so system runs once
        EntityManager.CreateEntity(typeof(UpdateOnce));
    }

    protected override void OnUpdate()
    {
        // Destroy singleton to prevent system from running again
        EntityManager.DestroyEntity(GetSingletonEntity<UpdateOnce>());
        var network = World.GetExistingSystem<NetworkStreamReceiveSystem>();
        if (World.GetExistingSystem<ClientSimulationSystemGroup>() != null)
        {
            string remoteServerIpAddress = GameSession.clientSession.remoteServerIpAddress;
            NetworkEndPoint ep = NetworkEndPoint.Parse(remoteServerIpAddress, GameSession.clientSession.remoteServerPort, NetworkFamily.Ipv4);
            
            Debug.Log("Connecting to " + remoteServerIpAddress + ":" + GameSession.clientSession.remoteServerPort);
            
            network.Connect(ep);
        }
        else if (World.GetExistingSystem<ServerSimulationSystemGroup>() != null)
        {
            // Server world automatically listens for connections from any host
            NetworkEndPoint ep = NetworkEndPoint.AnyIpv4;
            ep.Port = GameSession.serverSession.serverPort;
            network.Listen(ep);
        }
    }
}