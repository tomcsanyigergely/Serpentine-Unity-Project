using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
public class PlayerReadyServerSystem : ComponentSystem
{
    private List<int> playersReady = new List<int>();
    
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref PlayerReadyRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            var playerId = EntityManager.GetComponentData<NetworkIdComponent>(reqSrc.SourceConnection).Value;

            if (!playersReady.Exists((int id) => id == playerId))
            {
                playersReady.Add(playerId);
                
                if (playersReady.Count == GameSession.serverSession.numberOfPlayers)
                {
                    Entities.WithAny<NetworkIdComponent>().ForEach((Entity connectionEntity) =>
                    {
                        var countdownStartedRequest = PostUpdateCommands.CreateEntity();
                        PostUpdateCommands.AddComponent(countdownStartedRequest, new CountdownStartedRequest{ CountdownSeconds = SerializedFields.singleton.startCountdownSeconds });
                        PostUpdateCommands.AddComponent(countdownStartedRequest, new SendRpcCommandRequestComponent { TargetConnection = connectionEntity});
                    });

                    var startCountdownEntity = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(startCountdownEntity, new StartCountdownComponent { RemainingTime = SerializedFields.singleton.startCountdownSeconds });
                }
            }
        });
    }
}
