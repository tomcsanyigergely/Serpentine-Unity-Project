using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct ProgressionComponent : IComponentData
{
    [GhostDefaultField]
    public uint CrossedCheckpoints;
}
