using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class DropPowerupServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref DropPowerupRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);
            
            var playerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;

            uint slotNumber = req.slotNumber;

            Entities.ForEach((Entity carEntity, ref SynchronizedCarComponent synchronizedCarComponent, DynamicBuffer <PowerupSlotElement> powerupSlots) => {
                if (synchronizedCarComponent.PlayerId == playerId)
                {
                    if (slotNumber < SerializedFields.singleton.numberOfPowerupSlots)
                    {
                        if (powerupSlots[(int)slotNumber].Content == PowerupSlotContent.Laser)
                        {
                            var laserPowerupSlots = EntityManager.GetBuffer<LaserPowerupSlotElement>(carEntity);

                            for (int i = 0; i < laserPowerupSlots.Length; i++)
                            {
                                if (laserPowerupSlots[i].SlotNumber == slotNumber)
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

                                    break;
                                }
                            }
                        }
                        
                        powerupSlots[(int)slotNumber] = new PowerupSlotElement { Content = PowerupSlotContent.Empty };

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
                }
            });
        });
    }
}