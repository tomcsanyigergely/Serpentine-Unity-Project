using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Events;

public class PlayerDiedRequestClientSystem : ComponentSystem
{
    public UnityAction OnPlayerDied;
    
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref PlayerDiedRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);

            if (req.PlayerId == GetSingleton<NetworkIdComponent>().Value)
            {
                OnPlayerDied?.Invoke();
            }
        });
    }
}
