using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostDefaultComponent(targetType: GhostDefaultComponentAttribute.Type.Server)]
public struct ActiveComponent : IComponentData
{
    [GhostDefaultField]
    public bool IsActive;
}
