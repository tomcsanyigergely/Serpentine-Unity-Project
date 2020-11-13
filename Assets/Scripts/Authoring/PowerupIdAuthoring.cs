using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(UniqueId))]
public class PowerupIdAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var powerupIdInitializationSystem = dstManager.World.GetOrCreateSystem<PowerupIdInitializationSystem>();
        if (powerupIdInitializationSystem != null)
        {
            powerupIdInitializationSystem.powerups.Add(GetComponent<UniqueId>().id, entity);
        }
    }
}
