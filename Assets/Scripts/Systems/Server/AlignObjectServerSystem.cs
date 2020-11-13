using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

/*
 * System responsible for aligning game objects to the surface.
 * This is a simplified implementation, works only for horizontal
 * surfaces with y = 0 in world-space.
 */

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateAfter(typeof(PostTriggerEventServerSystem))]
[UpdateBefore(typeof(SerpentineGhostSendSystem))]
public class AlignObjectServerSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAny<CarTag, ProjectileTag>().ForEach((ref Translation position, ref Rotation rotation, ref PhysicsVelocity velocity) =>
        {
            Quaternion currentOrientation = rotation.Value;
            Vector3 transformUp = currentOrientation * Vector3.up;

            Vector3 positionOnSurface = new float3(position.Value.x, 0, position.Value.z);

            Vector3 surfaceNormal = Vector3.up;

            Vector3 rotationAxis = Vector3.Cross(transformUp, surfaceNormal);
            Rotation newRotation = rotation;
            if (rotationAxis.magnitude > 0)
            {
                rotationAxis = rotationAxis.normalized;
                Quaternion rotationQuaternion = Quaternion.AngleAxis(Vector3.Angle(transformUp, surfaceNormal), rotationAxis);
                newRotation.Value = rotationQuaternion * currentOrientation;
            }

            Vector3 velocityVector = velocity.Linear;
            Vector3 angularVelocityVector = velocity.Angular;

            velocityVector = velocityVector - math.dot(velocityVector, surfaceNormal) * Vector3.up;
            angularVelocityVector = math.dot(angularVelocityVector, Vector3.up) * Vector3.up;

            position = new Translation {Value = positionOnSurface + surfaceNormal * 0.5f};
            rotation = new Rotation {Value = newRotation.Value};
            velocity = new PhysicsVelocity {Linear = velocityVector, Angular = angularVelocityVector};
        });
    }
}
