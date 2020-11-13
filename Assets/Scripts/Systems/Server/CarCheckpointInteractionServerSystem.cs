using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(InteractionServerSystemGroup))]
public class CarCheckpointInteractionServerSystem : ComponentSystem
{
    private CheckpointInitializationSystem checkpointInitializationSystem;

    private uint finishedPlayers = 0;

    protected override void OnCreate()
    {
        checkpointInitializationSystem = World.GetOrCreateSystem<CheckpointInitializationSystem>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity interactionEntity, ref CarCheckpointInteractionComponent interaction) =>
        {
            PostUpdateCommands.DestroyEntity(interactionEntity);

            var checkpointNumber = EntityManager.GetComponentData<CheckpointComponent>(interaction.Checkpoint).CheckpointNumber;
            var carProgressionComponent = EntityManager.GetComponentData<ProgressionComponent>(interaction.Car);
            var nextCheckpointNumberOfCar = (carProgressionComponent.CrossedCheckpoints + 1) % checkpointInitializationSystem.numberOfCheckpoints;

            if (checkpointNumber == nextCheckpointNumberOfCar)
            {
                carProgressionComponent.CrossedCheckpoints++;

                EntityManager.SetComponentData(interaction.Car, carProgressionComponent);

                if (carProgressionComponent.CrossedCheckpoints == checkpointInitializationSystem.numberOfCheckpoints * GameSession.serverSession.laps)
                {
                    var playerId = EntityManager.GetComponentData<SynchronizedCarComponent>(interaction.Car).PlayerId;

                    finishedPlayers++;
                    
                    Entities.ForEach((Entity connectionEntity, ref NetworkIdComponent networkIdComponent) =>
                    {
                        if (playerId == networkIdComponent.Value)
                        {
                            Debug.Log("Player " + playerId + " finished!");
                            
                            var playerFinishedRequest = PostUpdateCommands.CreateEntity();
                            PostUpdateCommands.AddComponent(playerFinishedRequest, new PlayerFinishedRequest { Position = finishedPlayers });
                            PostUpdateCommands.AddComponent(playerFinishedRequest, new SendRpcCommandRequestComponent { TargetConnection = connectionEntity });
                        }
                    });
                }
            }
        });
    }
}
