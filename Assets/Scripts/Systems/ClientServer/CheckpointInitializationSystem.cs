using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInWorld(UpdateInWorld.TargetWorld.ClientAndServer)]
public class CheckpointInitializationSystem : ComponentSystem
{
    private struct UpdateOnce : IComponentData
    {
    }

    public uint numberOfCheckpoints;

    public Entity[] checkpoints;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<UpdateOnce>();
        EntityManager.CreateEntity(typeof(UpdateOnce));
    }

    protected override void OnUpdate()
    {
        EntityManager.DestroyEntity(GetSingletonEntity<UpdateOnce>());

        checkpoints = new Entity[numberOfCheckpoints];
        
        Entities.ForEach((Entity entity, ref CheckpointComponent checkpointComponent) =>
        {
            checkpoints[checkpointComponent.CheckpointNumber] = entity;
        });
    }
}
