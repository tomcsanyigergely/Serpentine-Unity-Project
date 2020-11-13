using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInWorld(UpdateInWorld.TargetWorld.ClientAndServer)]
public class PowerupIdInitializationSystem : ComponentSystem
{
    private struct UpdateOnce : IComponentData
    {
    }

    public SortedDictionary<string, Entity> powerups = new SortedDictionary<string, Entity>();

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<UpdateOnce>();
        EntityManager.CreateEntity(typeof(UpdateOnce));
    }

    protected override void OnUpdate()
    {
        Dictionary<Entity, uint> idByPowerupEntity = new Dictionary<Entity, uint>();

        uint nextPowerupId = 0;
        
        foreach (var pair in powerups)
        {
            idByPowerupEntity.Add(pair.Value, nextPowerupId);
            nextPowerupId++;
        }
        
        EntityManager.DestroyEntity(GetSingletonEntity<UpdateOnce>());

        Entities.WithAny<PowerupTag>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.AddComponent(entity, typeof(PowerupIdComponent));
            PostUpdateCommands.SetComponent(entity, new PowerupIdComponent{ PowerupId = idByPowerupEntity[entity] });
        });
    }
}
