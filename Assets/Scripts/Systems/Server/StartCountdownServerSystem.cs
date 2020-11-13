using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class StartCountdownServerSystem : ComponentSystem
{
    public bool raceHasStarted = false;
    
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref StartCountdownComponent startCountdownComponent) =>
        {
            startCountdownComponent.RemainingTime -= 1f / 60;
            if (startCountdownComponent.RemainingTime <= 0)
            {
                PostUpdateCommands.DestroyEntity(entity);

                raceHasStarted = true;
                
                Entities.WithAny<NetworkIdComponent>().ForEach((Entity connectionEntity) =>
                {
                    var raceStartedRequest = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(raceStartedRequest, new RaceStartedRequest{});
                    PostUpdateCommands.AddComponent(raceStartedRequest, new SendRpcCommandRequestComponent{ TargetConnection = connectionEntity });
                });
            }
        });
    }
}
