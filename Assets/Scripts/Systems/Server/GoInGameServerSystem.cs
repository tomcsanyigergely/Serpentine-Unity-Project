using Unity.Entities;
using Unity.NetCode;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class GoInGameServerSystem : ComponentSystem
{
    public uint connectedPlayers = 0;

    private List<uint> freeSpawnLocationIndices;

    protected override void OnCreate()
    {
        freeSpawnLocationIndices = new List<uint>();
        for (uint i = 0; i < GameSession.serverSession.numberOfPlayers; i++)
        {
            freeSpawnLocationIndices.Add(i);
        }
    }

    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref GoInGameRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            if (connectedPlayers < GameSession.serverSession.numberOfPlayers)
            {
                PostUpdateCommands.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
                var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
                var ghostId = SerpentineGhostSerializerCollection.FindGhostType<CarStubSnapshotData>();
                var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
                var player = PostUpdateCommands.Instantiate(prefab);

                int indexOfNextFreeSpawnLocationIndex = Random.Range(0, freeSpawnLocationIndices.Count);
                uint nextSpawnLocationIndex = freeSpawnLocationIndices[indexOfNextFreeSpawnLocationIndex];
                freeSpawnLocationIndices.RemoveAt(indexOfNextFreeSpawnLocationIndex);
                
                PostUpdateCommands.SetComponent(player, new Translation{ Value = SerializedFields.singleton.spawnLocations[(int)nextSpawnLocationIndex].position });
                PostUpdateCommands.SetComponent(player, new Rotation{ Value = SerializedFields.singleton.spawnLocations[(int)nextSpawnLocationIndex].rotation });
                PostUpdateCommands.SetComponent(player, new SynchronizedCarComponent {PlayerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value});
                PostUpdateCommands.SetComponent(player, new MissileScopeComponent {TargetPlayerId = null});
                PostUpdateCommands.AddBuffer<PlayerInput>(player);
                PostUpdateCommands.AddBuffer<PowerupSlotElement>(player);
                PostUpdateCommands.AddBuffer<LaserPowerupSlotElement>(player);

                for (int i = 0; i < SerializedFields.singleton.numberOfPowerupSlots; i++)
                {
                    PostUpdateCommands.AppendToBuffer(player, new PowerupSlotElement {Content = PowerupSlotContent.Empty});
                }

                PostUpdateCommands.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent {targetEntity = player});
                PostUpdateCommands.DestroyEntity(reqEnt);

                var raceInformationRequest = PostUpdateCommands.CreateEntity();
                PostUpdateCommands.AddComponent(raceInformationRequest, new RaceInformationRequest{ laps = GameSession.serverSession.laps });
                PostUpdateCommands.AddComponent(raceInformationRequest, new SendRpcCommandRequestComponent { TargetConnection = reqSrc.SourceConnection });
                
                connectedPlayers++;
            }
        });
    }
}
