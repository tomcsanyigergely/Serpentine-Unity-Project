using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
[UpdateAfter(typeof(GhostSimulationSystemGroup))]
public class ProjectileLightClientSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer addProjectileLightComponentBuffer = new EntityCommandBuffer(Allocator.Temp, PlaybackPolicy.SinglePlayback);
        
        // Executing on projectiles, which have just been instantiated:
        Entities.WithAll<ProjectileTag>().WithNone<ProjectileLightSystemStateComponent>().ForEach((Entity projectileEntity) =>
        {
            Entity projectileLightEntity = addProjectileLightComponentBuffer.Instantiate(PrefabEntities.projectileLightPrefabEntity);
            addProjectileLightComponentBuffer.AddComponent(projectileEntity, new ProjectileLightSystemStateComponent
            {
                ProjectileLightEntity = projectileLightEntity
            });
        });

        addProjectileLightComponentBuffer.Playback(EntityManager);
        
        // Executing on already existing projectiles:
        Entities.WithAll<ProjectileTag, ProjectileLightSystemStateComponent>().ForEach((ref Translation position, ref ProjectileLightSystemStateComponent projectileLight) =>
        {
            EntityManager.SetComponentData(projectileLight.ProjectileLightEntity, position);
        });
        
        // Executing on destroyed projectiles:
        Entities.WithAll<ProjectileLightSystemStateComponent>().WithNone<ProjectileTag>().ForEach((Entity projectileEntity, ref ProjectileLightSystemStateComponent projectileLight) =>
        {
            PostUpdateCommands.DestroyEntity(projectileLight.ProjectileLightEntity);
            PostUpdateCommands.RemoveComponent<ProjectileLightSystemStateComponent>(projectileEntity);
        });
    }
}
