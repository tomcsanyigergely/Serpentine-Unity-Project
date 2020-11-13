using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class ShieldServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref ShieldComponent shieldComponent, ref SynchronizedCarComponent synchronizedCarComponent) => {
            if (shieldComponent.RemainingTime > 0)
            {
                shieldComponent.RemainingTime -= 1 / 60f;
                if (shieldComponent.RemainingTime <= 0)
                {
                    shieldComponent.RemainingTime = 0;
                    synchronizedCarComponent.IsShieldActive = false;
                }
            }
        });
    }
}
