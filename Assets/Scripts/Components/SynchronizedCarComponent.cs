using Unity.Entities;
using Unity.NetCode;

[GenerateAuthoringComponent]
public struct SynchronizedCarComponent : IComponentData
{
    [GhostDefaultField]
    public int PlayerId;

    [GhostDefaultField(quantizationFactor: 10)]
    public float SteerAngle;

    [GhostDefaultField]
    public bool IsShieldActive;
}