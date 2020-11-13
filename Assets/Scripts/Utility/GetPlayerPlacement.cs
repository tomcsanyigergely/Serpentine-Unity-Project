using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public static partial class Utils
{
    private struct PlayerDistance
    {
        public int PlayerId;
        public float Distance;
    }
    
    public static uint GetPlayerPlacement(int playerId, uint numberOfPlayers, EntityManager EntityManager, EntityQueryBuilder Entities, ComponentSystem componentSystem)
    {
        SortedDictionary<uint, List<Entity>> carEntitiesByCrossedCheckpoints = new SortedDictionary<uint, List<Entity>>();

        Entities.ForEach((Entity carEntity, ref ProgressionComponent progressionComponent) =>
        {
            if (!carEntitiesByCrossedCheckpoints.ContainsKey(progressionComponent.CrossedCheckpoints))
            {
                carEntitiesByCrossedCheckpoints.Add(progressionComponent.CrossedCheckpoints, new List<Entity>());
            }

            carEntitiesByCrossedCheckpoints[progressionComponent.CrossedCheckpoints].Add(carEntity);
        });
        
        uint skippedPlayers = 0;
        
        foreach (var pair in carEntitiesByCrossedCheckpoints)
        {
            var carEntitiesInSameSection = pair.Value;

            if (carEntitiesInSameSection.Exists((Entity car) => EntityManager.GetComponentData<SynchronizedCarComponent>(car).PlayerId == playerId))
            {
                if (carEntitiesInSameSection.Count == 1)
                {
                    return numberOfPlayers - skippedPlayers;
                }
                else
                {
                    NativeArray<CalculateCheckpointDistanceJob.Input> jobInput = new NativeArray<CalculateCheckpointDistanceJob.Input>(carEntitiesInSameSection.Count * 2, Allocator.TempJob);
                    NativeArray<CalculateCheckpointDistanceJob.Output> jobOutput = new NativeArray<CalculateCheckpointDistanceJob.Output>(carEntitiesInSameSection.Count * 2, Allocator.TempJob);

                    var crossedCheckpoints = pair.Key;

                    var checkPointInitializationSystem = componentSystem.World.GetExistingSystem<CheckpointInitializationSystem>();
                    var numberOfCheckpoints = checkPointInitializationSystem.numberOfCheckpoints;

                    var nextCheckpointEntity = checkPointInitializationSystem.checkpoints[(crossedCheckpoints + 1) % numberOfCheckpoints];
                    var nextCheckpointPosition = EntityManager.GetComponentData<Translation>(nextCheckpointEntity).Value;
                    var nextCheckpointRotation = EntityManager.GetComponentData<Rotation>(nextCheckpointEntity).Value;
                    var nextCheckpointScale = EntityManager.GetComponentData<NonUniformScale>(nextCheckpointEntity).Value;
                    
                    var nextNextCheckpointEntity = checkPointInitializationSystem.checkpoints[(crossedCheckpoints + 2) % numberOfCheckpoints];
                    var nextNextCheckpointPosition = EntityManager.GetComponentData<Translation>(nextNextCheckpointEntity).Value;
                    var nextNextCheckpointRotation = EntityManager.GetComponentData<Rotation>(nextNextCheckpointEntity).Value;
                    var nextNextCheckpointScale = EntityManager.GetComponentData<NonUniformScale>(nextNextCheckpointEntity).Value;
                    
                    for (int i = 0; i < carEntitiesInSameSection.Count; i++)
                    {
                        var carEntity = carEntitiesInSameSection[i];
                        var carPosition = EntityManager.GetComponentData<Translation>(carEntity).Value;
                        var carRotation = EntityManager.GetComponentData<Rotation>(carEntity).Value;
                        
                        jobInput[2 * i + 0] = new CalculateCheckpointDistanceJob.Input
                        {
                            CarPosition = carPosition,
                            CarRotation = carRotation,
                            CheckPointPosition = nextCheckpointPosition,
                            CheckPointRotation = nextCheckpointRotation,
                            CheckPointSize = nextCheckpointScale
                        };

                        jobInput[2 * i + 1] = new CalculateCheckpointDistanceJob.Input
                        {
                            CarPosition = carPosition,
                            CarRotation = carRotation,
                            CheckPointPosition = nextNextCheckpointPosition,
                            CheckPointRotation = nextNextCheckpointRotation,
                            CheckPointSize = nextNextCheckpointScale
                        };
                    }

                    var job = new CalculateCheckpointDistanceJob
                    {
                        input = jobInput,
                        output = jobOutput
                    };

                    JobHandle jobHandle = job.Schedule(carEntitiesInSameSection.Count * 2, 1);
                    
                    jobHandle.Complete();

                    List<PlayerDistance> playerDistancesFromNextCheckpoint = new List<PlayerDistance>();

                    for (int i = 0; i < carEntitiesInSameSection.Count; i++)
                    {
                        playerDistancesFromNextCheckpoint.Add(new PlayerDistance
                        {
                            PlayerId = EntityManager.GetComponentData<SynchronizedCarComponent>(carEntitiesInSameSection[i]).PlayerId,
                            Distance = jobOutput[2 * i + 0].Distance
                        });
                    }

                    uint placement = 0;

                    if (playerDistancesFromNextCheckpoint.Exists((element) => element.PlayerId == playerId && element.Distance == 0) &&
                        playerDistancesFromNextCheckpoint.Exists((element) => element.PlayerId != playerId && element.Distance == 0))
                    {
                        List<PlayerDistance> playerDistancesFromNextNextCheckpoint = new List<PlayerDistance>();

                        for (int i = 0; i < playerDistancesFromNextCheckpoint.Count; i++)
                        {
                            if (playerDistancesFromNextCheckpoint[i].Distance == 0)
                            {
                                playerDistancesFromNextNextCheckpoint.Add(new PlayerDistance
                                {
                                    PlayerId = playerDistancesFromNextCheckpoint[i].PlayerId,
                                    Distance = jobOutput[2 * i + 1].Distance
                                });
                            }
                        }

                        skippedPlayers += Convert.ToUInt32(playerDistancesFromNextCheckpoint.Count - playerDistancesFromNextNextCheckpoint.Count);
                        
                        playerDistancesFromNextNextCheckpoint.Sort((lhs, rhs) => rhs.Distance.CompareTo(lhs.Distance)); // ORDER BY distance DESC;
                        
                        foreach (var playerDistance in playerDistancesFromNextNextCheckpoint)
                        {
                            if (playerDistance.PlayerId != playerId)
                            {
                                skippedPlayers++;
                            }
                            else
                            {
                                placement = numberOfPlayers - skippedPlayers;
                                break;
                            }
                        }
                    }
                    else
                    {
                        playerDistancesFromNextCheckpoint.Sort((lhs, rhs) => rhs.Distance.CompareTo(lhs.Distance)); // ORDER BY distance DESC;

                        foreach (var playerDistance in playerDistancesFromNextCheckpoint)
                        {
                            if (playerDistance.PlayerId != playerId)
                            {
                                skippedPlayers++;
                            }
                            else
                            {
                                placement = numberOfPlayers - skippedPlayers;
                                break;
                            }
                        }
                    }

                    jobInput.Dispose();
                    jobOutput.Dispose();

                    return placement;
                }
            }
            else
            {
                skippedPlayers += Convert.ToUInt32(carEntitiesInSameSection.Count);
            }
        }
        
        return 0;
    }

    [BurstCompile]
    private struct CalculateCheckpointDistanceJob : IJobParallelFor
    {
        public struct Input
        {
            public float3 CarPosition;
            public quaternion CarRotation;
            public float3 CheckPointPosition;
            public quaternion CheckPointRotation;
            public float3 CheckPointSize;
        }

        public struct Output
        {
            public float Distance;
        }

        [ReadOnly] public NativeArray<Input> input;
        [WriteOnly] public NativeArray<Output> output;
        
        [BurstCompile]
        public unsafe void Execute(int index)
        {
            PhysicsCollider carBoxCollider = new PhysicsCollider
            {
                Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
                {
                    Center = input[index].CarPosition,
                    Orientation = input[index].CarRotation,
                    Size = new float3(1.8f, 2.0f, 4.2f),
                    BevelRadius = 0
                })
            };

            PhysicsCollider checkpointCollider = new PhysicsCollider
            {
                Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
                {
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = input[index].CheckPointSize,
                    BevelRadius = 0
                })
            };
            
            carBoxCollider.ColliderPtr->CalculateDistance(new ColliderDistanceInput
            {
                Collider = checkpointCollider.ColliderPtr,
                Transform = new RigidTransform
                {
                    pos = input[index].CheckPointPosition,
                    rot = input[index].CheckPointRotation
                },
                MaxDistance = math.distance(input[index].CarPosition, input[index].CheckPointPosition) + 50
            }, out DistanceHit hit);

            output[index] = new Output {Distance = hit.Distance};
        }
    }
}
