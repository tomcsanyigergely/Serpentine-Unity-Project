using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Events;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class PlayerFinishedClientSystem : ComponentSystem
{
    public UnityAction<uint> OnPlayerFinished;
    
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref PlayerFinishedRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);

            OnPlayerFinished?.Invoke(req.Position);
        });
    }
}
