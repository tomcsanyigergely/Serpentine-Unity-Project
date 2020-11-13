using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostDefaultComponent(targetType: GhostDefaultComponentAttribute.Type.Server)]
public struct CarComponent : IComponentData
{
    public bool GoingBackward;
}
