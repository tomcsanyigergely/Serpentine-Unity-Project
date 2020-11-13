using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PrefabEntities : MonoBehaviour, IConvertGameObjectToEntity
{
    public static Entity projectileLightPrefabEntity;
    
    [SerializeField] private GameObject projectileLightPrefab;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        using (BlobAssetStore blobAssetStore = new BlobAssetStore())
        {
            projectileLightPrefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                projectileLightPrefab, GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
        }
    }
}
