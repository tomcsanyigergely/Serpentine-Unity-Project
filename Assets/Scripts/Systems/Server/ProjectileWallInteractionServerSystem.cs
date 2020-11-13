using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(InteractionServerSystemGroup))]
public class ProjectileWallInteractionServerSystem : ComponentSystem
{
    private InteractionServerSystemGroup interactionServerSystemGroup;
    
    protected override void OnCreate()
    {
        interactionServerSystemGroup = World.GetOrCreateSystem<InteractionServerSystemGroup>();
    }
    
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity interactionEntity, ref ProjectileWallInteractionComponent interaction) =>
        {
            PostUpdateCommands.DestroyEntity(interactionEntity);

            if (EntityManager.GetComponentData<ActiveComponent>(interaction.Projectile).IsActive)
            {
                EntityManager.SetComponentData(interaction.Projectile, new ActiveComponent { IsActive = false });
                interactionServerSystemGroup.PostUpdateCommands.DestroyEntity(interaction.Projectile);
            }
        });
    }
}
