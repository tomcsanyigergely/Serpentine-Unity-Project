using Unity.Entities;

public struct MissileCarInteractionComponent : IComponentData
{
    public Entity Missile;
    public Entity Car;
}
