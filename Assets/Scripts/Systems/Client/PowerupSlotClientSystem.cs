using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Events;
using Unity.Collections.LowLevel.Unsafe;
using System;

[UpdateInGroup(typeof(ClientPresentationSystemGroup))]
public unsafe class PowerupSlotClientSystem : ComponentSystem
{
    public UnityAction<uint, PowerupSlotContent, PowerupSlotChangedRequest.PowerupSlotData> OnPowerupSlotChanged;
    public UnityAction OnMissilePowerupAcquired;
    public UnityAction OnAllMissilePowerupsUsed;

    public PowerupSlotContent[] slots;

    protected override void OnCreate()
    {
        slots = new PowerupSlotContent[3];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = PowerupSlotContent.Empty;
        }
    }

    protected override void OnUpdate()
    {
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, ref PowerupSlotChangedRequest req, ref ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            PostUpdateCommands.DestroyEntity(reqEnt);

            var previousSlotContent = slots[req.SlotNumber];

            slots[req.SlotNumber] = req.SlotContent;

            PowerupSlotChangedRequest.PowerupSlotData slotData = null;

            switch(req.SlotContent)
            {
                case PowerupSlotContent.Laser:
                    UnsafeUtility.CopyPtrToStructure(req.SlotData, out PowerupSlotChangedRequest.LaserPowerupSlotData laserPowerupSlotData);
                    slotData = laserPowerupSlotData;
                    break;
            }

            if (req.SlotData != null)
            {
                UnsafeUtility.Free(req.SlotData, Unity.Collections.Allocator.Persistent);
            }

            OnPowerupSlotChanged?.Invoke(req.SlotNumber, req.SlotContent, slotData);

            if (req.SlotContent == PowerupSlotContent.Missile)
            {
                OnMissilePowerupAcquired?.Invoke();
            }
            else if (previousSlotContent == PowerupSlotContent.Missile)
            {
                bool hasMissilePowerups = false;
                foreach (var slot in slots)
                {
                    if (slot == PowerupSlotContent.Missile)
                    {
                        hasMissilePowerups = true;
                        break;
                    }
                }

                if (!hasMissilePowerups)
                {
                    OnAllMissilePowerupsUsed?.Invoke();
                }
            }
        });
    }
}
