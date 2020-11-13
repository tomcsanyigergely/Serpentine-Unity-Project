using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Events;

[UpdateInGroup(typeof(ClientPresentationSystemGroup))]
public unsafe class MissileTargetClientSystem : ComponentSystem
{
    public UnityAction OnMissileTargetChanged;
    public UnityAction OnMissileTargetLost;
    public UnityAction OnMissileTargetFound;

    public int? MissileTargetPlayerId = null;

    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref MissileTargetChangedRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);

            int? previousTargetPlayerId = MissileTargetPlayerId;

            if (req.HasTarget)
            {
                MissileTargetPlayerId = req.TargetPlayerId;
            }
            else
            {
                MissileTargetPlayerId = null;
            }

            OnMissileTargetChanged?.Invoke();

            if (MissileTargetPlayerId.HasValue && !previousTargetPlayerId.HasValue)
            {
                OnMissileTargetFound?.Invoke();
            }
            else if (!MissileTargetPlayerId.HasValue && previousTargetPlayerId.HasValue)
            {
                OnMissileTargetLost?.Invoke();
            }
        });
    }
}

