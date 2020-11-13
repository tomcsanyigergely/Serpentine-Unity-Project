using Unity.Entities;

public struct ProjectileCarInteractionComponent : IComponentData
{
    public Entity Projectile;
    public Entity Car;
}
