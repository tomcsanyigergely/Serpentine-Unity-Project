using Unity.Entities;

[InternalBufferCapacity(3)]
public struct LaserPowerupSlotElement : IBufferElementData
{
    public uint SlotNumber;
    public uint RemainingShots;
}
