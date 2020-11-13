using Unity.Entities;
using Unity.NetCode;

[UpdateInWorld(UpdateInWorld.TargetWorld.Server)]
public class FixedRateServerSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();
        EntityManager.CreateEntity(typeof(ClientServerTickRate));
        SetSingleton<ClientServerTickRate>(new ClientServerTickRate
        {
            MaxSimulationStepsPerFrame = 60,
            NetworkTickRate = 60,
            SimulationTickRate = 60,
            TargetFrameRateMode = ClientServerTickRate.FrameRateMode.Auto
        });
    }

    protected override void OnUpdate()
    {
        
    }
}