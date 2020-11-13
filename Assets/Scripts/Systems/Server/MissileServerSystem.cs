using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class MissileServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity missileEntity, ref MissileTargetComponent missileTargetComponent, ref Translation position, ref Rotation rotation, ref PhysicsVelocity velocity) =>
        {
            missileTargetComponent.RemainingTime -= 1f / 60;
            if (missileTargetComponent.RemainingTime > 0)
            {
                Rotation carRotation = EntityManager.GetComponentData<Rotation>(missileTargetComponent.TargetEntity);
                Translation carPosition = EntityManager.GetComponentData<Translation>(missileTargetComponent.TargetEntity);
                float3 carUp = math.mul(carRotation.Value, new float3(0, 1, 0));
                float3 carToMissileVector = position.Value - carPosition.Value;

                float3 carToVirtualTargetNormalized = Vector3.RotateTowards(carUp, carToMissileVector, SerializedFields.singleton.missileVirtualTargetMaxAngle / 180.0f * math.PI, 0);
                float3 virtualTarget = carPosition.Value + carToVirtualTargetNormalized * SerializedFields.singleton.missileVirtualTargetMaxDistance * math.min(math.length(carToMissileVector) / SerializedFields.singleton.missileVirtualTargetLerpDistance, 1);
                
                float3 transformForward = math.mul(rotation.Value, new float3(0, 1, 0)); // the missile's forward direction is its transform's up vector
                float3 missileToTarget = math.normalize(virtualTarget - position.Value);

                float3 newTransformForward = Vector3.RotateTowards(transformForward, missileToTarget, 1 / 60f * SerializedFields.singleton.missileRotationSpeed, 0);
                quaternion rotateTowards = Quaternion.FromToRotation(transformForward, newTransformForward);

                rotation.Value = math.mul(rotateTowards, rotation.Value);
                velocity.Linear = newTransformForward * (math.length(velocity.Linear) + SerializedFields.singleton.missileAcceleration * 1f / 60);
            }
            else
            {
                PostUpdateCommands.DestroyEntity(missileEntity);
            }
        });
    }
}
