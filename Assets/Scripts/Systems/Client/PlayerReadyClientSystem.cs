using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class PlayerReadyClientSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
    }

    public void SendPlayerReadyRequest()
    {
        var playerReadyRequest = EntityManager.CreateEntity();
        EntityManager.AddComponentData(playerReadyRequest, new PlayerReadyRequest());
        EntityManager.AddComponentData(playerReadyRequest, new SendRpcCommandRequestComponent { TargetConnection = GetSingletonEntity<NetworkIdComponent>() });
    }
}
