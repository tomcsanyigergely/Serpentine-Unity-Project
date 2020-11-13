using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InteractionServerSystemGroup))]
public class MissileRoadInteractionServerSystem : ComponentSystem
{
    private InteractionServerSystemGroup interactionServerSystemGroup;
    
    protected override void OnCreate()
    {
        interactionServerSystemGroup = World.GetOrCreateSystem<InteractionServerSystemGroup>();
    }
    
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity interactionEntity, ref MissileRoadInteractionComponent interaction) => {
            PostUpdateCommands.DestroyEntity(interactionEntity);

            if (EntityManager.GetComponentData<ActiveComponent>(interaction.Missile).IsActive)
            {
                EntityManager.SetComponentData(interaction.Missile, new ActiveComponent {IsActive = false});
                interactionServerSystemGroup.PostUpdateCommands.DestroyEntity(interaction.Missile);

                Debug.Log("Missile collided with Road: " + EntityManager.GetComponentData<Translation>(interaction.Missile).Value);
            }
        });
    }
}
