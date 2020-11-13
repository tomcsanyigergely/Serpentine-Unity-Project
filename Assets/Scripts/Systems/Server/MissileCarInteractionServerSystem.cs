using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InteractionServerSystemGroup))]
[UpdateBefore(typeof(MissileRoadInteractionServerSystem))]
public class MissileCarInteractionServerSystem : ComponentSystem
{
    private InteractionServerSystemGroup interactionServerSystemGroup;
    
    protected override void OnCreate()
    {
        interactionServerSystemGroup = World.GetOrCreateSystem<InteractionServerSystemGroup>();
    }
    
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity interactionEntity, ref MissileCarInteractionComponent interaction) => {
            PostUpdateCommands.DestroyEntity(interactionEntity);

            if (EntityManager.GetComponentData<ActiveComponent>(interaction.Missile).IsActive)
            {
                var carPlayerId = EntityManager.GetComponentData<SynchronizedCarComponent>(interaction.Car).PlayerId;
            
                if (carPlayerId != EntityManager.GetComponentData<OwnerComponent>(interaction.Missile).OwnerPlayerId)
                {
                    EntityManager.SetComponentData(interaction.Missile, new ActiveComponent {IsActive = false});
                    interactionServerSystemGroup.PostUpdateCommands.DestroyEntity(interaction.Missile);

                    if (!EntityManager.GetComponentData<SynchronizedCarComponent>(interaction.Car).IsShieldActive)
                    {
                        var healthComponent = EntityManager.GetComponentData<HealthComponent>(interaction.Car);

                        if (healthComponent.Health > 0)
                        {
                            healthComponent.Health -= SerializedFields.singleton.missileDamage;
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
                        float3 impactPoint = EntityManager.GetComponentData<Translation>(interaction.Missile).Value - EntityManager.GetComponentData<PhysicsVelocity>(interaction.Missile).Linear * 1 / 60f + new float3(UnityEngine.Random.Range(-1, +1), UnityEngine.Random.Range(-1, +1), UnityEngine.Random.Range(-1, +1));
                    
                        carVelocity.ApplyImpulse(
                            EntityManager.GetComponentData<PhysicsMass>(interaction.Car),
                            carPosition,
                            carRotation,
                            math.normalize(Vector3.ProjectOnPlane(EntityManager.GetComponentData<PhysicsVelocity>(interaction.Missile).Linear, carTransformUp)) * SerializedFields.singleton.missileImpulse,
                            impactPoint - (float3)(Vector3.Project(impactPoint - carPosition.Value, carTransformUp))
                        );
                    
                        EntityManager.SetComponentData(interaction.Car, carVelocity);
                    }
                }
            }
        });
    }
}
