using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
[GhostDefaultComponent(targetType:GhostDefaultComponentAttribute.Type.Server)]
public struct OwnerComponent : IComponentData
{
    public int OwnerPlayerId;
}
    
