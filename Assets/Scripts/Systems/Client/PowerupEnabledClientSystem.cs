using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class PowerupEnabledClientSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref PowerupEnabledRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);

            var powerupId = req.PowerupId;
            var enabled = req.Enabled;

            Entities.With(EntityQueryOptions.IncludeDisabled).ForEach((Entity powerupEntity, ref PowerupIdComponent powerupIdComponent) => {
                if (powerupId == powerupIdComponent.PowerupId)
                {
                    if (enabled)
                    {
                        PostUpdateCommands.RemoveComponent(powerupEntity, typeof(Disabled));
                    }
                    else
                    {
                        PostUpdateCommands.AddComponent(powerupEntity, typeof(Disabled));
                    }
                }
            });
        });
    }
}
