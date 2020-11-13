using Unity.Entities;

public struct ResurrectionComponent : IComponentData
{
    public Entity CarToResurrect;
    public float RemainingTime;
}
