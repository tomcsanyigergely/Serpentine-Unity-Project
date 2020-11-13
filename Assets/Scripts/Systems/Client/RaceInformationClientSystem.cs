using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Events;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class RaceInformationClientSystem : ComponentSystem
{
    public UnityAction OnRaceInformationReceived;
    
    public uint laps = 0;
    
    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref RaceInformationRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);

            laps = req.laps;
            
            OnRaceInformationReceived?.Invoke();
        });
    }
}
