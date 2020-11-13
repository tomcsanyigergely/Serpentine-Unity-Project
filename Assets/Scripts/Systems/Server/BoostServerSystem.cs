using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class BoostServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref BoostComponent boostComponent) =>
        {
            if (boostComponent.RemainingTime > 0)
            {
                boostComponent.RemainingTime -= 1f / 60;
                if (boostComponent.RemainingTime < 0)
                {
                    boostComponent.RemainingTime = 0;
                }
            }
        });
    }
}
