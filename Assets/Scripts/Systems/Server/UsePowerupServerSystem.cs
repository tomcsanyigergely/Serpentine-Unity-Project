using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public unsafe class UsePowerupServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref UsePowerupRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            var playerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;

            uint slotNumber = req.slotNumber;

            Entities.ForEach((Entity carEntity, DynamicBuffer <PowerupSlotElement> powerupSlots, ref HealthComponent healthComponent, ref ShieldComponent shieldComponent, ref SynchronizedCarComponent carComponent, ref Translation position, ref Rotation rotation) => {
                if (carComponent.PlayerId == playerId && healthComponent.Health > 0)
                {
                    if (slotNumber < SerializedFields.singleton.numberOfPowerupSlots)
                    {
                        switch (powerupSlots[(int)slotNumber].Content)
                        {
                            case PowerupSlotContent.Laser:
                                var laserPowerupSlots = EntityManager.GetBuffer<LaserPowerupSlotElement>(carEntity);

                                for (int i = 0; i < laserPowerupSlots.Length; i++)
                                {
                                    if (laserPowerupSlots[i].SlotNumber == slotNumber)
                                    {
                                        var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
                                        var ghostId = SerpentineGhostSerializerCollection.FindGhostType<ProjectileSnapshotData>();
                                        var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;

                                        var projectile = PostUpdateCommands.Instantiate(prefab);
                                        PostUpdateCommands.SetComponent(projectile, new OwnerComponent { OwnerPlayerId = carComponent.PlayerId });
                                        
                                        PostUpdateCommands.SetComponent(projectile, new Translation { Value = position.Value + math.forward(rotation.Value) * 3f });
                                        PostUpdateCommands.SetComponent(projectile, new Rotation { Value = rotation.Value });
                                        PostUpdateCommands.SetComponent(projectile, new PhysicsVelocity { Angular = float3.zero, Linear = math.forward(rotation.Value) * SerializedFields.singleton.projectileRelativeSpeed + EntityManager.GetComponentData<PhysicsVelocity>(carEntity).Linear });
                                        
                                        if (laserPowerupSlots[i].RemainingShots > 1)
                                        {
                                            laserPowerupSlots[i] = new LaserPowerupSlotElement
                                            {
                                                SlotNumber = laserPowerupSlots[i].SlotNumber,
                                                RemainingShots = laserPowerupSlots[i].RemainingShots - 1
                                            };

                                            void* slotData = UnsafeUtility.Malloc(sizeof(PowerupSlotChangedRequest.LaserPowerupSlotData), sizeof(PowerupSlotChangedRequest.LaserPowerupSlotData), Unity.Collections.Allocator.Persistent);
                                            var laserPowerupSlotData = new PowerupSlotChangedRequest.LaserPowerupSlotData { RemainingShots = laserPowerupSlots[i].RemainingShots };
                                            UnsafeUtility.CopyStructureToPtr(ref laserPowerupSlotData, slotData);

                                            Entities.ForEach((Entity ent, ref NetworkIdComponent id) =>
                                            {
                                                if (id.Value == playerId)
                                                {
                                                    var request = PostUpdateCommands.CreateEntity();
                                                    PostUpdateCommands.AddComponent(request, new PowerupSlotChangedRequest { SlotNumber = slotNumber, SlotContent = PowerupSlotContent.Laser, SlotData = slotData });
                                                    PostUpdateCommands.AddComponent(request, new SendRpcCommandRequestComponent { TargetConnection = ent });
                                                }
                                            });
                                        }
                                        else
                                        {
                                            powerupSlots[(int)slotNumber] = new PowerupSlotElement { Content = PowerupSlotContent.Empty };

                                            laserPowerupSlots.RemoveAt(i);

                                            Entities.ForEach((Entity ent, ref NetworkIdComponent id) =>
                                            {
                                                if (id.Value == playerId)
                                                {
                                                    var request = PostUpdateCommands.CreateEntity();
                                                    PostUpdateCommands.AddComponent(request, new PowerupSlotChangedRequest { SlotNumber = slotNumber, SlotContent = PowerupSlotContent.Empty });
                                                    PostUpdateCommands.AddComponent(request, new SendRpcCommandRequestComponent { TargetConnection = ent });
                                                }
                                            });
                                        }

                                        break;
                                    }
                                }                                

                                break;
                            case PowerupSlotContent.Boost:
                                if (Utils.GetPlayerPlacement(carComponent.PlayerId, GameSession.serverSession.numberOfPlayers, EntityManager, Entities, this) != 1)
                                {
                                    powerupSlots[(int) slotNumber] = new PowerupSlotElement {Content = PowerupSlotContent.Empty};

                                    var boostComponent = EntityManager.GetComponentData<BoostComponent>(carEntity);

                                    boostComponent.RemainingTime = math.max(boostComponent.RemainingTime, SerializedFields.singleton.boostPowerupDuration);

                                    EntityManager.SetComponentData(carEntity, boostComponent);

                                    Entities.ForEach((Entity ent, ref NetworkIdComponent id) =>
                                    {
                                        if (id.Value == playerId)
                                        {
                                            var request = PostUpdateCommands.CreateEntity();
                                            PostUpdateCommands.AddComponent(request, new PowerupSlotChangedRequest {SlotNumber = slotNumber, SlotContent = PowerupSlotContent.Empty});
                                            PostUpdateCommands.AddComponent(request, new SendRpcCommandRequestComponent {TargetConnection = ent});
                                        }
                                    });
                                }

                                break;
                            case PowerupSlotContent.Shield:
                                powerupSlots[(int)slotNumber] = new PowerupSlotElement { Content = PowerupSlotContent.Empty };

                                carComponent.IsShieldActive = true;
                                shieldComponent.RemainingTime = math.max(SerializedFields.singleton.shieldPowerupDuration, shieldComponent.RemainingTime);

                                Entities.ForEach((Entity ent, ref NetworkIdComponent id) =>
                                {
                                    if (id.Value == playerId)
                                    {
                                        var request = PostUpdateCommands.CreateEntity();
                                        PostUpdateCommands.AddComponent(request, new PowerupSlotChangedRequest { SlotNumber = slotNumber, SlotContent = PowerupSlotContent.Empty });
                                        PostUpdateCommands.AddComponent(request, new SendRpcCommandRequestComponent { TargetConnection = ent });
                                    }
                                });

                                break;
                            case PowerupSlotContent.Repair:
                                powerupSlots[(int)slotNumber] = new PowerupSlotElement { Content = PowerupSlotContent.Empty };

                                healthComponent.Health += SerializedFields.singleton.repairPowerupHealth;
                                if (healthComponent.Health > SerializedFields.singleton.maxHealth)
                                {
                                    healthComponent.Health = SerializedFields.singleton.maxHealth;
                                }

                                Entities.ForEach((Entity ent, ref NetworkIdComponent id) =>
                                {
                                    if (id.Value == playerId)
                                    {
                                        var request = PostUpdateCommands.CreateEntity();
                                        PostUpdateCommands.AddComponent(request, new PowerupSlotChangedRequest { SlotNumber = slotNumber, SlotContent = PowerupSlotContent.Empty });
                                        PostUpdateCommands.AddComponent(request, new SendRpcCommandRequestComponent { TargetConnection = ent });
                                    }
                                });

                                break;
                            case PowerupSlotContent.Missile:
                                if (EntityManager.GetComponentData<MissileScopeComponent>(carEntity).TargetPlayerId.HasValue)
                                {
                                    powerupSlots[(int) slotNumber] = new PowerupSlotElement
                                    {
                                        Content = PowerupSlotContent.Empty
                                    };
                                    
                                    var ghostCollection = GetSingleton<GhostPrefabCollectionComponent>();
                                    var ghostId = SerpentineGhostSerializerCollection.FindGhostType<MissileSnapshotData>();
                                    var prefab = EntityManager.GetBuffer<GhostPrefabBuffer>(ghostCollection.serverPrefabs)[ghostId].Value;
                                    
                                    float3 carTransformRight = math.mul(rotation.Value, new float3(1, 0, 0));
                                    float3 carTransformUp = math.mul(rotation.Value, new float3(0, 1, 0));
                                    float3 carTransformForward = math.mul(rotation.Value, new float3(0, 0, 1));
                                    quaternion rotateUpward = Quaternion.AngleAxis(-30, carTransformRight);

                                    for (int i = 0; i < SerializedFields.singleton.numberOfMissilesOnShot; i++)
                                    {
                                        var missile = PostUpdateCommands.Instantiate(prefab);
                                        PostUpdateCommands.SetComponent(missile, new OwnerComponent {OwnerPlayerId = carComponent.PlayerId});

                                        PostUpdateCommands.SetComponent(missile, new Translation {Value = position.Value + carTransformUp * 2f + carTransformForward * 1f});
                                        PostUpdateCommands.SetComponent(missile, new Rotation {Value = math.mul(Quaternion.AngleAxis((i-(SerializedFields.singleton.numberOfMissilesOnShot-1)/2f) * 10f, carTransformUp), math.mul(Quaternion.AngleAxis(90, carTransformRight), math.mul(rotateUpward, rotation.Value)))});
                                        PostUpdateCommands.SetComponent(missile, new PhysicsVelocity {Angular = float3.zero, Linear = math.mul(Quaternion.AngleAxis((i-(SerializedFields.singleton.numberOfMissilesOnShot-1)/2f) * 10f, carTransformUp), math.mul(rotateUpward, carTransformForward) * (SerializedFields.singleton.missileRelativeSpeed + math.max(0, math.dot(EntityManager.GetComponentData<PhysicsVelocity>(carEntity).Linear, carTransformForward))))});

                                        Entities.ForEach((Entity otherCarEntity, ref SynchronizedCarComponent synchronizedCarComponent) =>
                                        {
                                            if (synchronizedCarComponent.PlayerId == EntityManager.GetComponentData<MissileScopeComponent>(carEntity).TargetPlayerId.Value)
                                            {
                                                PostUpdateCommands.SetComponent(missile, new MissileTargetComponent {TargetEntity = otherCarEntity, RemainingTime = SerializedFields.singleton.missileLifetime});
                                            }
                                        });
                                    }

                                    Entities.ForEach((Entity ent, ref NetworkIdComponent id) =>
                                    {
                                        if (id.Value == playerId)
                                        {
                                            var request = PostUpdateCommands.CreateEntity();
                                            PostUpdateCommands.AddComponent(request, new PowerupSlotChangedRequest {SlotNumber = slotNumber, SlotContent = PowerupSlotContent.Empty});
                                            PostUpdateCommands.AddComponent(request, new SendRpcCommandRequestComponent {TargetConnection = ent});
                                        }
                                    });
                                }

                                break;
                        }
                    }
                }
            });

            PostUpdateCommands.DestroyEntity(reqEnt);
        });
    }
}