using Unity.Entities;

public struct PowerupCarInteractionComponent : IComponentData
{
    public Entity Powerup;
    public Entity Car;
}
