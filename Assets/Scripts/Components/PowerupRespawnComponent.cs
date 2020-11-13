using Unity.Entities;

public struct PowerupRespawnComponent : IComponentData
{
    public uint PowerupId;
    public float RemainingTime;
}
