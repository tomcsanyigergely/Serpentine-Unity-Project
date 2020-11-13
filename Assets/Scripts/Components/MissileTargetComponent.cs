using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostDefaultComponent(targetType:GhostDefaultComponentAttribute.Type.Server)]
public struct MissileTargetComponent : IComponentData
{
    public Entity TargetEntity;
    public float RemainingTime;
}
