using Unity.Entities;

public struct ProjectileWallInteractionComponent : IComponentData
{
    public Entity Projectile;
    public Entity Wall;
}
