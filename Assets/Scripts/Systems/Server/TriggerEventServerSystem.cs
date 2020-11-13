using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.NetCode;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public class TriggerEventServerSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBufferSystem;
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    private struct TriggerEventJob : ITriggerEventsJob
    {       
        [ReadOnly] public ComponentDataFromEntity<CarTag> cars;
        [ReadOnly] public ComponentDataFromEntity<ProjectileTag> projectiles;
        [ReadOnly] public ComponentDataFromEntity<PowerupTag> powerups;
        [ReadOnly] public ComponentDataFromEntity<WallTag> walls;
        [ReadOnly] public ComponentDataFromEntity<MissileTag> missiles;
        [ReadOnly] public ComponentDataFromEntity<RoadTag> roads;
        [ReadOnly] public ComponentDataFromEntity<CheckpointTag> checkpoints;
        public EntityCommandBuffer commandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            if (projectiles.HasComponent(triggerEvent.EntityA) && cars.HasComponent(triggerEvent.EntityB))
            {
                CreateProjectileCarInteraction(triggerEvent.EntityA, triggerEvent.EntityB);
            }
            else if (projectiles.HasComponent(triggerEvent.EntityB) && cars.HasComponent(triggerEvent.EntityA))
            {
                CreateProjectileCarInteraction(triggerEvent.EntityB, triggerEvent.EntityA);
            }
            else if (powerups.HasComponent(triggerEvent.EntityA) && cars.HasComponent(triggerEvent.EntityB))
            {
                CreatePowerupCarInteraction(triggerEvent.EntityA, triggerEvent.EntityB);
            }
            else if (powerups.HasComponent(triggerEvent.EntityB) && cars.HasComponent(triggerEvent.EntityA))
            {
                CreatePowerupCarInteraction(triggerEvent.EntityB, triggerEvent.EntityA);
            }    
            else if (projectiles.HasComponent(triggerEvent.EntityA) && walls.HasComponent(triggerEvent.EntityB))
            {
                CreateProjectileWallInteraction(triggerEvent.EntityA, triggerEvent.EntityB);
            }
            else if (projectiles.HasComponent(triggerEvent.EntityB) && walls.HasComponent(triggerEvent.EntityA))
            {
                CreateProjectileWallInteraction(triggerEvent.EntityB, triggerEvent.EntityA);
            }
            else if (missiles.HasComponent(triggerEvent.EntityA) && cars.HasComponent(triggerEvent.EntityB))
            {
                CreateMissileCarInteraction(triggerEvent.EntityA, triggerEvent.EntityB);
            }
            else if (missiles.HasComponent(triggerEvent.EntityB) && cars.HasComponent(triggerEvent.EntityA))
            {
                CreateMissileCarInteraction(triggerEvent.EntityB, triggerEvent.EntityA);
            }
            else if (missiles.HasComponent(triggerEvent.EntityA) && roads.HasComponent(triggerEvent.EntityB))
            {
                CreateMissileRoadInteraction(triggerEvent.EntityA, triggerEvent.EntityB);
            }
            else if (missiles.HasComponent(triggerEvent.EntityB) && roads.HasComponent(triggerEvent.EntityA))
            {
                CreateMissileRoadInteraction(triggerEvent.EntityB, triggerEvent.EntityA);
            }
            else if (cars.HasComponent(triggerEvent.EntityA) && checkpoints.HasComponent(triggerEvent.EntityB))
            {
                CreateCarCheckpointInteraction(triggerEvent.EntityA, triggerEvent.EntityB);
            }
            else if (cars.HasComponent(triggerEvent.EntityB) && checkpoints.HasComponent(triggerEvent.EntityA))
            {
                CreateCarCheckpointInteraction(triggerEvent.EntityB, triggerEvent.EntityA);
            }
        }

        private void CreateProjectileCarInteraction(Entity projectile, Entity car)
        {
            Entity interaction = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(interaction, new ProjectileCarInteractionComponent
            {
                Projectile = projectile,
                Car = car
            });
        }

        private void CreatePowerupCarInteraction(Entity powerup, Entity car)
        {
            Entity interaction = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(interaction, new PowerupCarInteractionComponent
            {
                Powerup = powerup,
                Car = car
            });
        }

        private void CreateProjectileWallInteraction(Entity projectile, Entity wall)
        {
            Entity interaction = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(interaction, new ProjectileWallInteractionComponent
            {
                Projectile = projectile,
                Wall = wall
            });
        }

        private void CreateMissileCarInteraction(Entity missile, Entity car)
        {
            Entity interaction = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(interaction, new MissileCarInteractionComponent
            {
                Missile = missile,
                Car = car
            });
        }

        private void CreateMissileRoadInteraction(Entity missile, Entity road)
        {
            Entity interaction = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(interaction, new MissileRoadInteractionComponent
            {
                Missile = missile,
                Road = road
            });
        }

        private void CreateCarCheckpointInteraction(Entity car, Entity checkpoint)
        {
            Entity interaction = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(interaction, new CarCheckpointInteractionComponent
            {
                Car = car,
                Checkpoint = checkpoint
            });
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        commandBufferSystem = World.GetOrCreateSystem<PostTriggerEventServerSystem>();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new TriggerEventJob
        {
            cars = GetComponentDataFromEntity<CarTag>(),
            projectiles = GetComponentDataFromEntity<ProjectileTag>(),
            powerups = GetComponentDataFromEntity<PowerupTag>(),
            walls = GetComponentDataFromEntity<WallTag>(),
            missiles = GetComponentDataFromEntity<MissileTag>(),
            roads = GetComponentDataFromEntity<RoadTag>(),
            checkpoints = GetComponentDataFromEntity<CheckpointTag>(),
            commandBuffer = commandBufferSystem.CreateCommandBuffer()
        }.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(job);
        return job;
    }
}
