using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

[UpdateInGroup(typeof(InteractionServerSystemGroup))]
public class PowerupCarInteractionServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity interactionEntity, ref PowerupCarInteractionComponent interaction) => {
            PostUpdateCommands.DestroyEntity(interactionEntity);

            if (EntityManager.GetComponentData<ActiveComponent>(interaction.Powerup).IsActive)
            {
                DynamicBuffer<PowerupSlotElement> powerupSlots = EntityManager.GetBuffer<PowerupSlotElement>(interaction.Car);

                for (uint slotNumber = 0; slotNumber < powerupSlots.Length; slotNumber++)
                {
                    if (powerupSlots[(int)slotNumber].Content == PowerupSlotContent.Empty)
                    {
                        var powerupId = EntityManager.GetComponentData<PowerupIdComponent>(interaction.Powerup).PowerupId;

                        EntityManager.SetComponentData(interaction.Powerup, new ActiveComponent { IsActive = false });
                        PostUpdateCommands.AddComponent(interaction.Powerup, typeof(Disabled));
                        var ghostRespawnEntity = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(ghostRespawnEntity, new PowerupRespawnComponent { PowerupId = powerupId, RemainingTime = SerializedFields.singleton.powerupRespawnTime });

                        PowerupSlotContent slotContent = (PowerupSlotContent)((int)EntityManager.GetComponentData<PowerupComponent>(interaction.Powerup).Powerup);

                        unsafe
                        {
                            void* powerupSlotData = null;

                            switch (slotContent)
                            {
                                case PowerupSlotContent.Laser:
                                    PostUpdateCommands.AppendToBuffer(interaction.Car, new LaserPowerupSlotElement {SlotNumber = slotNumber, RemainingShots = SerializedFields.singleton.laserPowerupShots});

                                    var laserPowerupSlotData = new PowerupSlotChangedRequest.LaserPowerupSlotData {RemainingShots = SerializedFields.singleton.laserPowerupShots};
                                    powerupSlotData = UnsafeUtility.Malloc(sizeof(PowerupSlotChangedRequest.LaserPowerupSlotData), sizeof(PowerupSlotChangedRequest.LaserPowerupSlotData), Unity.Collections.Allocator.Persistent);
                                    UnsafeUtility.CopyStructureToPtr(ref laserPowerupSlotData, powerupSlotData);
                                    break;
                            }

                            powerupSlots[(int) slotNumber] = new PowerupSlotElement {Content = slotContent};

                            var car = interaction.Car;

                            Entities.ForEach((Entity connectionEntity, ref NetworkIdComponent id) =>
                            {
                                var ghostEnabledRequest = PostUpdateCommands.CreateEntity();
                                PostUpdateCommands.AddComponent(ghostEnabledRequest, new PowerupEnabledRequest {PowerupId = powerupId, Enabled = false});
                                PostUpdateCommands.AddComponent(ghostEnabledRequest, new SendRpcCommandRequestComponent {TargetConnection = connectionEntity});

                                if (id.Value == EntityManager.GetComponentData<SynchronizedCarComponent>(car).PlayerId)
                                {
                                    var powerupSlotChangedRequest = PostUpdateCommands.CreateEntity();
                                    PostUpdateCommands.AddComponent(powerupSlotChangedRequest, new PowerupSlotChangedRequest {SlotNumber = slotNumber, SlotContent = slotContent, SlotData = powerupSlotData});
                                    PostUpdateCommands.AddComponent(powerupSlotChangedRequest, new SendRpcCommandRequestComponent {TargetConnection = connectionEntity});
                                }
                            });
                        }

                        break;
                    }
                }
            }
        });
    }
}
