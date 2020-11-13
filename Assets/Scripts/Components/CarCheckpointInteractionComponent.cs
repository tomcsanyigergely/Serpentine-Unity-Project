using Unity.Entities;

public struct CarCheckpointInteractionComponent : IComponentData
{
    public Entity Car;
    public Entity Checkpoint;
}
