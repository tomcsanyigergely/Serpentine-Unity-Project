using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PowerupSlotUpdater : MonoBehaviour
{
    [SerializeField] private PowerupSlot[] slots;

    private PowerupSlotClientSystem powerupSlotClientSystem;
    private MissileTargetClientSystem missileTargetClientSystem;

    private void Awake()
    {
        foreach (World world in World.All)
        {
            PowerupSlotClientSystem powerupSlotClientSystem = world.GetExistingSystem<PowerupSlotClientSystem>();
            if (powerupSlotClientSystem != null)
            {
                this.powerupSlotClientSystem = powerupSlotClientSystem;
            }

            MissileTargetClientSystem missileTargetClientSystem = world.GetExistingSystem<MissileTargetClientSystem>();
            if (missileTargetClientSystem != null)
            {
                this.missileTargetClientSystem = missileTargetClientSystem;
            }

            PlacementUpdateSystem placementUpdateSystem = world.GetExistingSystem<PlacementUpdateSystem>();
            if (placementUpdateSystem != null)
            {
                placementUpdateSystem.OnUpdatePlacement += OnUpdatePlacement;
            }
        }        
    }

    private void Start()
    {
        for (int i = 0; i < powerupSlotClientSystem.slots.Length; i++)
        {
            var slot = slots[i];

            switch (powerupSlotClientSystem.slots[i])
            {
                case PowerupSlotContent.Empty:
                    slot.SetScale(0);
                    break;

                case PowerupSlotContent.Laser:
                    slot.SetSprite(SerializedFields.singleton.laserPowerupSprite);
                    slot.SetColor(SerializedFields.singleton.laserPowerupColor);

                    slot.SetScale(1);
                    break;

                case PowerupSlotContent.Boost:
                    slot.SetSprite(SerializedFields.singleton.boostPowerupSprite);
                    slot.SetColor(SerializedFields.singleton.boostPowerupColor);
                    slot.SetScale(1);
                    break;

                case PowerupSlotContent.Shield:
                    slot.SetSprite(SerializedFields.singleton.shieldPowerupSprite);
                    slot.SetColor(SerializedFields.singleton.shieldPowerupColor);
                    slot.SetScale(1);
                    break;

                case PowerupSlotContent.Repair:
                    slot.SetSprite(SerializedFields.singleton.repairPowerupSprite);
                    slot.SetColor(SerializedFields.singleton.repairPowerupColor);
                    slot.SetScale(1);
                    break;

                case PowerupSlotContent.Missile:
                    slot.SetSprite(SerializedFields.singleton.missilePowerupSprite);
                    slot.SetColor(missileTargetClientSystem.MissileTargetPlayerId.HasValue ? SerializedFields.singleton.missilePowerupColor : SerializedFields.singleton.inactivePowerupColor);
                    slot.SetScale(1);
                    break;
            }
        }
    }

    public void OnPowerupSlotChanged(uint slotNumber, PowerupSlotContent slotContent, PowerupSlotChangedRequest.PowerupSlotData slotData)
    {
        PowerupSlot slot = slots[slotNumber];

        switch (slotContent)
        {
            case PowerupSlotContent.Empty:
                slot.SetScale(0);
                break;

            case PowerupSlotContent.Laser:
                slot.SetSprite(SerializedFields.singleton.laserPowerupSprite);
                slot.SetColor(SerializedFields.singleton.laserPowerupColor);

                PowerupSlotChangedRequest.LaserPowerupSlotData laserPowerupSlotData = (PowerupSlotChangedRequest.LaserPowerupSlotData)slotData;
                slot.SetScale(laserPowerupSlotData.RemainingShots * 1.0f / SerializedFields.singleton.laserPowerupShots);
                break;

            case PowerupSlotContent.Boost:
                slot.SetSprite(SerializedFields.singleton.boostPowerupSprite);
                slot.SetColor(SerializedFields.singleton.boostPowerupColor);
                slot.SetScale(1);
                break;

            case PowerupSlotContent.Shield:
                slot.SetSprite(SerializedFields.singleton.shieldPowerupSprite);
                slot.SetColor(SerializedFields.singleton.shieldPowerupColor);
                slot.SetScale(1);
                break;

            case PowerupSlotContent.Repair:
                slot.SetSprite(SerializedFields.singleton.repairPowerupSprite);
                slot.SetColor(SerializedFields.singleton.repairPowerupColor);
                slot.SetScale(1);
                break;

            case PowerupSlotContent.Missile:
                slot.SetSprite(SerializedFields.singleton.missilePowerupSprite);
                slot.SetColor(missileTargetClientSystem.MissileTargetPlayerId.HasValue ? SerializedFields.singleton.missilePowerupColor : SerializedFields.singleton.inactivePowerupColor);
                slot.SetScale(1);
                break;
        }
    }

    private void OnUpdatePlacement(uint placement)
    {
        for (int i = 0; i < powerupSlotClientSystem.slots.Length; i++)
        {
            if (powerupSlotClientSystem.slots[i] == PowerupSlotContent.Boost)
            {
                slots[i].SetColor(placement != 1 ? SerializedFields.singleton.boostPowerupColor : SerializedFields.singleton.inactivePowerupColor);
            }
        }
    }

    public void OnMissileTargetLost()
    {
        for (int i = 0; i < powerupSlotClientSystem.slots.Length; i++)
        {
            if (powerupSlotClientSystem.slots[i] == PowerupSlotContent.Missile)
            {
                slots[i].SetColor(SerializedFields.singleton.inactivePowerupColor);
            }
        }
    }

    public void OnMissileTargetFound()
    {
        for (int i = 0; i < powerupSlotClientSystem.slots.Length; i++)
        {
            if (powerupSlotClientSystem.slots[i] == PowerupSlotContent.Missile)
            {                
                slots[i].SetColor(SerializedFields.singleton.missilePowerupColor);
            }
        }
    }
}
