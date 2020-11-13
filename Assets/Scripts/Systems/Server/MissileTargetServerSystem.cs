using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(MoveCarServerSystem))]
public class MissileTargetServerSystem : ComponentSystem
{
    private struct PlayerScope
    {
        public Entity CarEntity;
        public NativeList<TargetData> TargetList;
    }
    
    private struct TargetData
    {
        public int JobIndex;
        public int TargetPlayerId;
        public float TargetAngle;
    }
    
    private const int jobPoolSize = 100; // dont have to be more than n*(n-1), where n is the maximum number of players
    private NativeArray<CheckTargetVisibilityJob.Input>[] jobInputPool;
    private NativeArray<CheckTargetVisibilityJob.Output>[] jobOutputPool;
    
    // Allocating structs used for job communication early, because allocating and deallocating multiple NativeArrays every tick is costly:
    protected override void OnCreate()
    {
        jobInputPool = new NativeArray<CheckTargetVisibilityJob.Input>[jobPoolSize];
        jobOutputPool = new NativeArray<CheckTargetVisibilityJob.Output>[jobPoolSize];
        
        for (int i = 0; i < jobPoolSize; i++)
        {
            jobInputPool[i] = new NativeArray<CheckTargetVisibilityJob.Input>(1, Allocator.TempJob);
            jobOutputPool[i] = new NativeArray<CheckTargetVisibilityJob.Output>(1, Allocator.TempJob);
        }
    }
    
    protected override void OnUpdate()
    {
        List<PlayerScope> playerScopes = new List<PlayerScope>();
        NativeList<JobHandle> checkTargetVisibilityJobHandles = new NativeList<JobHandle>(Allocator.Temp);
        List<CheckTargetVisibilityJob> checkTargetVisibilityJobs = new List<CheckTargetVisibilityJob>();

        int nextJobIndex = 0;
        
        Entities.ForEach((Entity carEntity, ref SynchronizedCarComponent playerSynchronizedCarComponent, ref MissileScopeComponent missileScopeComponent, ref Translation position, ref Rotation rotation) =>
        {
            var targetList = new NativeList<TargetData>(Allocator.Persistent);
            
            playerScopes.Add(new PlayerScope
            {
                CarEntity = carEntity,
                TargetList = targetList
            });
            
            int playerId = playerSynchronizedCarComponent.PlayerId;
            float3 playerPosition = position.Value;
            quaternion playerRotation = rotation.Value;

            float3 playerTransformForward = math.mul(playerRotation, new float3(0, 0, 1));
            float3 playerTransformUp = math.mul(playerRotation, new float3(0, 1, 0));

            Entities.ForEach((Entity opponentCarEntity, ref SynchronizedCarComponent opponentSynchronizedCarComponent, ref Translation opponentPosition, ref HealthComponent healthComponent) => {
                if (healthComponent.Health > 0)
                {
                    int opponentPlayerId = opponentSynchronizedCarComponent.PlayerId;

                    if (playerId != opponentSynchronizedCarComponent.PlayerId)
                    {
                        float3 playerToOpponent = opponentPosition.Value - playerPosition;
                        float3 playerToOpponentProjected = Vector3.ProjectOnPlane(playerToOpponent, playerTransformUp);
                        float targetAngle = Vector3.Angle(playerTransformForward, playerToOpponentProjected);

                        if (targetAngle < SerializedFields.singleton.missileMaxTargetAngle && math.length(playerToOpponent) <= SerializedFields.singleton.missileMaxTargetDistance)
                        {
                            jobInputPool[nextJobIndex][0] = new CheckTargetVisibilityJob.Input
                            {
                                RaycastStart = playerPosition + 2 * playerTransformUp + (-5) * playerTransformForward,
                                RaycastEnd = opponentPosition.Value,
                                TargetEntity = opponentCarEntity
                            };
                            
                            CheckTargetVisibilityJob job = new CheckTargetVisibilityJob
                            {
                                physicsWorld = World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld.Clone(),
                                input = jobInputPool[nextJobIndex],
                                output = jobOutputPool[nextJobIndex]
                            };
                            
                            targetList.Add(new TargetData
                            {
                                JobIndex = nextJobIndex,
                                TargetPlayerId = opponentPlayerId,
                                TargetAngle = targetAngle
                            });

                            nextJobIndex++;
                            
                            checkTargetVisibilityJobs.Add(job);
                            checkTargetVisibilityJobHandles.Add(job.Schedule());
                        }
                    }
                }
            });
        });

        JobHandle.CompleteAll(checkTargetVisibilityJobHandles);

        foreach (var playerScope in playerScopes)
        {
            int? closestTargetPlayerId = null;
            float closestTargetAngle = 0;
            
            for (int i = 0; i < playerScope.TargetList.Length; i++)
            {
                int jobIndex = playerScope.TargetList[i].JobIndex;
                
                if (checkTargetVisibilityJobs[jobIndex].output[0].Visible)
                {
                    var targetPlayerId = playerScope.TargetList[i].TargetPlayerId;
                    var targetAngle = playerScope.TargetList[i].TargetAngle;

                    if (!closestTargetPlayerId.HasValue)
                    {
                        closestTargetPlayerId = targetPlayerId;
                        closestTargetAngle = targetAngle;
                    }
                    else if (targetAngle < closestTargetAngle)
                    {
                        closestTargetPlayerId = targetPlayerId;
                        closestTargetAngle = targetAngle;
                    }
                }
            }

            for (int i = 0; i < playerScope.TargetList.Length; i++)
            {
                int jobIndex = playerScope.TargetList[i].JobIndex;
                
                checkTargetVisibilityJobs[jobIndex].physicsWorld.Dispose();
            }

            playerScope.TargetList.Dispose();

            var missileScopeComponent = EntityManager.GetComponentData<MissileScopeComponent>(playerScope.CarEntity);

            if (missileScopeComponent.TargetPlayerId != closestTargetPlayerId)
            {
                missileScopeComponent.TargetPlayerId = closestTargetPlayerId;
                
                EntityManager.SetComponentData(playerScope.CarEntity, missileScopeComponent);

                var playerId = EntityManager.GetComponentData<SynchronizedCarComponent>(playerScope.CarEntity).PlayerId;

                Entities.ForEach((Entity connectionEntity, ref NetworkIdComponent networkIdComponent) =>
                {
                    if (networkIdComponent.Value == playerId)
                    {
                        var request = PostUpdateCommands.CreateEntity();

                        if (closestTargetPlayerId.HasValue)
                        {
                            PostUpdateCommands.AddComponent(request, new MissileTargetChangedRequest {HasTarget = true, TargetPlayerId = closestTargetPlayerId.Value});
                        }
                        else
                        {
                            PostUpdateCommands.AddComponent(request, new MissileTargetChangedRequest {HasTarget = false});
                        }

                        PostUpdateCommands.AddComponent(request, new SendRpcCommandRequestComponent {TargetConnection = connectionEntity});
                    }
                });
            }
        }
    }

    [BurstCompile]
    public struct CheckTargetVisibilityJob : IJob
    {
        public struct Input
        {
            public float3 RaycastStart;
            public float3 RaycastEnd;
            public Entity TargetEntity;
        }

        public struct Output
        {
            public bool Visible;
        }

        public NativeArray<Input> input;
        public NativeArray<Output> output;
        public PhysicsWorld physicsWorld;
        
        [BurstCompile]
        public void Execute()
        {
            var raycastInput = new RaycastInput
            {
                Start = input[0].RaycastStart,
                End = input[0].RaycastEnd,
                Filter = new CollisionFilter
                {
                    BelongsTo = 1 << 8,
                    CollidesWith = (1 << 4) | (1 << 0),
                    GroupIndex = 0
                }
            };

            physicsWorld.CastRay(raycastInput, out Unity.Physics.RaycastHit closestHit);

            output[0] = new Output
            {
                Visible = (closestHit.RigidBodyIndex == physicsWorld.GetRigidBodyIndex(input[0].TargetEntity))
            };
        }
    }
}
