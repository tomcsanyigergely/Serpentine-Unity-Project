using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Events;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class CountdownStartedClientSystem : ComponentSystem
{
    public UnityAction<uint> OnCountdownStarted;
    
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref CountdownStartedRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);
            
            OnCountdownStarted?.Invoke(req.CountdownSeconds);
        });
    }
}
