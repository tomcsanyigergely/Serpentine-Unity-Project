using Unity.Entities;
using Unity.NetCode;

[UpdateInWorld(UpdateInWorld.TargetWorld.Client)]
public class FixedRateClientSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        EntityManager.CreateEntity(typeof(FixedClientTickRate));
    }

    protected override void OnUpdate()
    {
        
    }
}
