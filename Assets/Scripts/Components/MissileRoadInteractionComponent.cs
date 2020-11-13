using Unity.Entities;

public struct MissileRoadInteractionComponent : IComponentData
{
    public Entity Missile;
    public Entity Road;
}