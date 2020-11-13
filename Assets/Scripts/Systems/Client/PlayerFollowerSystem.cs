using System;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
[UpdateAfter(typeof(AfterSimulationInterpolationSystem))]
public class PlayerFollowerSystem : ComponentSystem
{
    private uint tickToRender = 0;
    private double tickToRenderTime = 0;

    protected override void OnUpdate()
    {
        uint interpolationTick = World.GetExistingSystem<ClientSimulationSystemGroup>().InterpolationTick;

        if (tickToRender < interpolationTick - 4)
        {
            tickToRender = interpolationTick - 2;
            tickToRenderTime = Time.ElapsedTime;
            Debug.Log("Interpolation tick moved forward");
        }
        else if (tickToRender > interpolationTick)
        {
            tickToRender = interpolationTick - 2;
            tickToRenderTime = Time.ElapsedTime;
            Debug.Log("Interpolation tick moved backward");
        }

        Entities.WithNone<CurrentSimulatedPosition, CurrentSimulatedRotation>().ForEach((Entity entity, DynamicBuffer<CarStubSnapshotData> outputBuffer, ref SynchronizedCarComponent carComponent, ref Translation translation, ref Rotation rotation) => {
            EntityManager.AddComponent(entity, typeof(CurrentSimulatedPosition));
            EntityManager.AddComponent(entity, typeof(CurrentSimulatedRotation));

            GameObject carMesh = UnityEngine.Object.Instantiate(SerializedFields.singleton.carMeshPrefab);
            carMesh.GetComponent<PlayerFollower>().playerId = carComponent.PlayerId;

            World.GetExistingSystem<MissileTargetClientSystem>().OnMissileTargetChanged += carMesh.GetComponent<PlayerFollower>().OnMissileTargetChanged;
            World.GetExistingSystem<PowerupSlotClientSystem>().OnMissilePowerupAcquired += carMesh.GetComponent<PlayerFollower>().OnMissilePowerupAcquired;
            World.GetExistingSystem<PowerupSlotClientSystem>().OnAllMissilePowerupsUsed += carMesh.GetComponent<PlayerFollower>().OnAllMissilePowerupsUsed;

            if (carComponent.PlayerId == GetSingleton<NetworkIdComponent>().Value)
            {
                carMesh.GetComponent<PlayerFollower>().OnUpdateHealth += SerializedFields.singleton.localPlayerHealthUpdater.OnUpdateHealth;
                World.GetExistingSystem<PowerupSlotClientSystem>().OnPowerupSlotChanged += carMesh.GetComponent<PowerupSlotUpdater>().OnPowerupSlotChanged;
                World.GetExistingSystem<MissileTargetClientSystem>().OnMissileTargetFound += carMesh.GetComponent<PowerupSlotUpdater>().OnMissileTargetFound;
                World.GetExistingSystem<MissileTargetClientSystem>().OnMissileTargetLost += carMesh.GetComponent<PowerupSlotUpdater>().OnMissileTargetLost;

                SerializedFields.singleton.virtualCamera.Follow = carMesh.transform;
                SerializedFields.singleton.virtualCamera.LookAt = carMesh.transform;
            }
            else
            {
                carMesh.GetComponent<PowerupSlotUpdater>().enabled = false;
            }

            World.GetOrCreateSystem<PlacementUpdateSystem>().numberOfPlayers++;
        });

        tickToRender++;
        tickToRenderTime += 1 / 60d;
    }

    public Tuple<Vector3, Quaternion, float, float, bool> InterpolateCarState(int playerId, float time)
    {
        int previousTick = (int)tickToRender + Mathf.FloorToInt((time-(float)tickToRenderTime) / (1/60f));
        int nextTick = previousTick + 1;
        float t = (time - ((float)tickToRenderTime + (previousTick - tickToRender) * 1/60f)) / (1/60f);

        if (previousTick >= 0)
        {
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            float steerAngle = 0;
            float health = 0;
            bool isShieldActive = false;

            Entities.ForEach((DynamicBuffer<CarStubSnapshotData> outputBuffer, ref SynchronizedCarComponent carComponent) =>
            {
                if (carComponent.PlayerId == playerId)
                {
                    CarStubSnapshotData previousTickSnapshot;
                    CarStubSnapshotData nextTickSnapshot;

                    outputBuffer.GetDataAtTick((uint)previousTick, out previousTickSnapshot);
                    outputBuffer.GetDataAtTick((uint)nextTick, out nextTickSnapshot);

                    previousTickSnapshot.Interpolate(ref nextTickSnapshot, t);

                    position = previousTickSnapshot.GetTranslationValue();
                    rotation = previousTickSnapshot.GetRotationValue();
                    steerAngle = previousTickSnapshot.GetSynchronizedCarComponentSteerAngle();
                    health = previousTickSnapshot.GetHealthComponentHealth();
                    isShieldActive = previousTickSnapshot.GetSynchronizedCarComponentIsShieldActive();
                }
            });

            return Tuple.Create(position, rotation, steerAngle, health, isShieldActive);
        }
        else
        {
            Debug.Log("InterpolatePlayerTransform(): Negative previousTick");
            return Tuple.Create<Vector3, Quaternion, float, float, bool>(Vector3.zero, Quaternion.identity, 0, 0, false);
        }
    }
}
