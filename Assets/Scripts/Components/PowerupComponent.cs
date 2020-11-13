using Unity.Entities;

public enum Powerup
{
    Default = 0,
    Laser = 1,
    Boost = 2,
    Shield = 3,
    Repair = 4,
    Missile = 5
}

[GenerateAuthoringComponent]
public struct PowerupComponent : IComponentData
{
    public Powerup Powerup;
}
