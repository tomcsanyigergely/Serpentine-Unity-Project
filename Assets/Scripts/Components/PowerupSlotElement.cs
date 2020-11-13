using Unity.Entities;

public enum PowerupSlotContent
{
    Empty = 0,
    Default = 0,
    Laser = 1,
    Boost = 2,
    Shield = 3,
    Repair = 4,
    Missile = 5
}

[InternalBufferCapacity(3)]
public struct PowerupSlotElement : IBufferElementData
{
    public PowerupSlotContent Content;
}
