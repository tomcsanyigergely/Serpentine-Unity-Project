using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostDefaultComponent(targetType:GhostDefaultComponentAttribute.Type.Server)]
public struct BoostComponent : IComponentData
{
    public float RemainingTime;
}
