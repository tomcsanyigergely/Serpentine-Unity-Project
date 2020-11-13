using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostDefaultComponent(targetType: GhostDefaultComponentAttribute.Type.Server)]
public struct MissileScopeComponent : IComponentData
{
    public int? TargetPlayerId;
}
