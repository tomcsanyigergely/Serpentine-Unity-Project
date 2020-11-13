using Unity.Entities;
using Unity.NetCode;

public static partial class Utils
{
    public static void KillPlayer(Entity playerCar, EntityManager EntityManager, EntityQueryBuilder Entities, EntityCommandBuffer PostUpdateCommands)
    {
        var playerId = EntityManager.GetComponentData<SynchronizedCarComponent>(playerCar).PlayerId;
        
        DynamicBuffer<PowerupSlotElement> powerupSlots = EntityManager.GetBuffer<PowerupSlotElement>(playerCar);

        for (int i = 0; i < SerializedFields.singleton.numberOfPowerupSlots; i++)
        {
            powerupSlots[i] = new PowerupSlotElement {Content = PowerupSlotContent.Empty};
        }

        EntityManager.GetBuffer<LaserPowerupSlotElement>(playerCar).Clear();

        Entities.ForEach((Entity connectionEntity, ref NetworkIdComponent id) =>
        {
            if (id.Value == EntityManager.GetComponentData<SynchronizedCarComponent>(playerCar).PlayerId)
            {
                for (uint i = 0; i < SerializedFields.singleton.numberOfPowerupSlots; i++)
                {
                    var powerupSlotChangedRequest = PostUpdateCommands.CreateEntity();
                    PostUpdateCommands.AddComponent(powerupSlotChangedRequest, new PowerupSlotChangedRequest {SlotNumber = i, SlotContent = PowerupSlotContent.Empty});
                    PostUpdateCommands.AddComponent(powerupSlotChangedRequest, new SendRpcCommandRequestComponent {TargetConnection = connectionEntity});
                }
            }
        });

        var resurrectionEntity = PostUpdateCommands.CreateEntity();
        PostUpdateCommands.AddComponent(resurrectionEntity, new ResurrectionComponent
        {
            CarToResurrect = playerCar,
            RemainingTime = SerializedFields.singleton.deathPenaltyInSeconds
        });

        Entities.ForEach((Entity connectionEntity, ref NetworkIdComponent networkIdComponent) =>
        {
            var playerDiedRequest = PostUpdateCommands.CreateEntity();
            PostUpdateCommands.AddComponent(playerDiedRequest, new PlayerDiedRequest { PlayerId = playerId });
            PostUpdateCommands.AddComponent(playerDiedRequest, new SendRpcCommandRequestComponent {TargetConnection = connectionEntity});
        });
    }
}
