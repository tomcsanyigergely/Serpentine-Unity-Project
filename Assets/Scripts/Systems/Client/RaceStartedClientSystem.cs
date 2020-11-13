using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Events;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class RaceStartedClientSystem : ComponentSystem
{
    public UnityAction OnRaceStarted;
    
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref RaceStartedRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);
            
            OnRaceStarted?.Invoke();
        });
    }
}
