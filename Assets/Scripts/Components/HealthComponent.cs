using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct HealthComponent : IComponentData
{
    [GhostDefaultField(quantizationFactor: 10)]
    public float Health;
}
