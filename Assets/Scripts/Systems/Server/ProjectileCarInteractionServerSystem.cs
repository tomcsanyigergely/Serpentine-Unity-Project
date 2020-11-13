using Unity.Entities;
using Unity.NetCode;
using Unity.Physics.Systems;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

[UpdateInGroup(typeof(InteractionServerSystemGroup))]
[UpdateBefore(typeof(ProjectileWallInteractionServerSystem))]
public class ProjectileCarInteractionServerSystem : ComponentSystem
{
    private InteractionServerSystemGroup interactionServerSystemGroup;
    
    protected override void OnCreate()
    {
        interactionServerSystemGroup = World.GetOrCreateSystem<InteractionServerSystemGroup>();
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity interactionEntity, ref ProjectileCarInteractionComponent interaction) => {
            PostUpdateCommands.DestroyEntity(interactionEntity);

            if (EntityManager.GetComponentData<ActiveComponent>(interaction.Projectile).IsActive)
            {
                var carPlayerId = EntityManager.GetComponentData<SynchronizedCarComponent>(interaction.Car).PlayerId;

                if (carPlayerId != EntityManager.GetComponentData<OwnerComponent>(interaction.Projectile).OwnerPlayerId)
                {
                    EntityManager.SetComponentData(interaction.Projectile, new ActiveComponent {IsActive = false});
                    interactionServerSystemGroup.PostUpdateCommands.DestroyEntity(interaction.Projectile);

                    if (!EntityManager.GetComponentData<SynchronizedCarComponent>(interaction.Car).IsShieldActive)
                    {
                        var healthComponent = EntityManager.GetComponentData<HealthComponent>(interaction.Car);

                        if (healthComponent.Health > 0)
                        {
                            healthComponent.Health -= SerializedFields.singleton.projectileDamage;

                            if (healthComponent.Health <= 0)
                            {
                                healthComponent.Health = 0;
                                
                                Utils.KillPlayer(interaction.Car, EntityManager, Entities, PostUpdateCommands);
                            }

                            EntityManager.SetComponentData(interaction.Car, healthComponent);
                        }

                        PhysicsVelocity carVelocity = EntityManager.GetComponentData<PhysicsVelocity>(interaction.Car);

                        Translation carPosition = EntityManager.GetComponentData<Translation>(interaction.Car);
                        Rotation carRotation = EntityManager.GetComponentData<Rotation>(interaction.Car);
                        float3 carTransformUp = math.mul(carRotation.Value, new float3(0, 1, 0));
                        float3 impactPoint = EntityManager.GetComponentData<Translation>(interaction.Projectile).Value - EntityManager.GetComponentData<PhysicsVelocity>(interaction.Projectile).Linear * 1 / 60f;

                        carVelocity.ApplyImpulse(
                            EntityManager.GetComponentData<PhysicsMass>(interaction.Car),
                            carPosition,
                            carRotation,
                            math.normalize(Vector3.ProjectOnPlane(EntityManager.GetComponentData<PhysicsVelocity>(interaction.Projectile).Linear, carTransformUp)) * SerializedFields.singleton.projectileImpulse,
                            impactPoint - (float3) (Vector3.Project(impactPoint - carPosition.Value, carTransformUp))
                        );

                        EntityManager.SetComponentData(interaction.Car, carVelocity);
                    }
                }
            }
        });
    }
}
