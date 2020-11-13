using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostDefaultComponent(targetType: GhostDefaultComponentAttribute.Type.Server)]
public struct ShieldComponent : IComponentData
{
    public float RemainingTime;
}
