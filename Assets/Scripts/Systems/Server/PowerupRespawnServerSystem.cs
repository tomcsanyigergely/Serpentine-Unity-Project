using System;
using Unity.NetCode;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class PowerupRespawnServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity powerupRespawnEntity, ref PowerupRespawnComponent powerupRespawnComponent) => {
            powerupRespawnComponent.RemainingTime -= 1 / 60f;

            if (powerupRespawnComponent.RemainingTime <= 0)
            {
                PostUpdateCommands.DestroyEntity(powerupRespawnEntity);

                var powerupId = powerupRespawnComponent.PowerupId;

                Entities.With(EntityQueryOptions.IncludeDisabled).ForEach((Entity powerupEntity, ref PowerupIdComponent powerupIdComponent) => {
                    if (powerupIdComponent.PowerupId == powerupId)
                    {
                        PostUpdateCommands.RemoveComponent(powerupEntity, typeof(Disabled));
                        if (EntityManager.HasComponent(powerupEntity, typeof(ActiveComponent)))
                        {
                            EntityManager.SetComponentData(powerupEntity, new ActiveComponent { IsActive = true });
                        }

                        Entities.ForEach((Entity connectionEntity, ref NetworkIdComponent id) =>
                        {
                            var powerupEnabledRequest = PostUpdateCommands.CreateEntity();
                            PostUpdateCommands.AddComponent(powerupEnabledRequest, new PowerupEnabledRequest { PowerupId = powerupId, Enabled = true });
                            PostUpdateCommands.AddComponent(powerupEnabledRequest, new SendRpcCommandRequestComponent { TargetConnection = connectionEntity });
                        });
                    }
                });
            }
        });
    }
}
