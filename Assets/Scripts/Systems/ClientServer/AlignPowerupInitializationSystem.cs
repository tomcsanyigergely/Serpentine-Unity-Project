using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Physics;

[UpdateInWorld(UpdateInWorld.TargetWorld.ClientAndServer)]
[UpdateAfter(typeof(ExportPhysicsWorld))]
public unsafe class AlignPowerupInitializationSystem : ComponentSystem
{
    private struct UpdateOnce : IComponentData
    {
    }

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<UpdateOnce>();
        EntityManager.CreateEntity(typeof(UpdateOnce));
    }

    protected override void OnUpdate()
    {
        EntityManager.DestroyEntity(GetSingletonEntity<UpdateOnce>());

        Entities.WithAny<PowerupTag>().ForEach((ref Translation position) => {
            var collider = new PhysicsCollider
            {
                Value = Unity.Physics.SphereCollider.Create(
                    new SphereGeometry
                    {
                        Center = float3.zero,
                        Radius = 0.1f
                    },
                    new CollisionFilter
                    {
                        BelongsTo = 1 << 5,
                        CollidesWith = 1 << 4,
                        GroupIndex = 0
                    }
                )
            };

            var distanceQueryInput = new ColliderDistanceInput
            {
                Collider = collider.ColliderPtr,
                Transform = new RigidTransform
                {
                    pos = position.Value,
                    rot = quaternion.identity
                },
                MaxDistance = 20
            };

            World.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld.CalculateDistance(distanceQueryInput, out DistanceHit hit);

            position = new Translation { Value = hit.Position + hit.SurfaceNormal * 3.5f };
        });
    }
}
