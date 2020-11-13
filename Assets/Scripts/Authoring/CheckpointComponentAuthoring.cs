using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CheckpointComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    private uint CheckpointNumber;

    public void SetCheckpointNumber(uint checkpointNumber)
    {
        CheckpointNumber = checkpointNumber;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent(entity, typeof(CheckpointComponent));
        dstManager.SetComponentData(entity, new CheckpointComponent{ CheckpointNumber = this.CheckpointNumber});
    }
}
