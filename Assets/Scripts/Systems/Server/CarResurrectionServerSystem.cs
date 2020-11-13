using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(MoveCarServerSystem))]
[UpdateBefore(typeof(UsePowerupRequest))]
public class CarResurrectionServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity resurrectionEntity, ref ResurrectionComponent resurrectionComponent) =>
        {
            resurrectionComponent.RemainingTime -= 1f / 60;
            if (resurrectionComponent.RemainingTime <= 0)
            {
                PostUpdateCommands.DestroyEntity(resurrectionEntity);

                EntityManager.SetComponentData(resurrectionComponent.CarToResurrect, new HealthComponent
                {
                    Health = SerializedFields.singleton.maxHealth
                });

                var synchronizedCarComponent = EntityManager.GetComponentData<SynchronizedCarComponent>(resurrectionComponent.CarToResurrect);
                synchronizedCarComponent.IsShieldActive = true;
                EntityManager.SetComponentData(resurrectionComponent.CarToResurrect, synchronizedCarComponent);
                EntityManager.SetComponentData(resurrectionComponent.CarToResurrect, new ShieldComponent
                {
                    RemainingTime = SerializedFields.singleton.resurrectionShieldDuration
                });
            }
        });
    }
}
