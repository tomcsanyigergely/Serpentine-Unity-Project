using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RoadTagCustomAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new RoadTag{});
    }
}
