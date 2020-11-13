using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public struct ProjectileGhostSerializer : IGhostSerializer<ProjectileSnapshotData>
{
    private ComponentType componentTypeActiveComponent;
    private ComponentType componentTypeOwnerComponent;
    private ComponentType componentTypeProjectileTag;
    private ComponentType componentTypePhysicsCollider;
    private ComponentType componentTypePhysicsGravityFactor;
    private ComponentType componentTypePhysicsMass;
    private ComponentType componentTypePhysicsVelocity;
    private ComponentType componentTypeCompositeScale;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<ActiveComponent> ghostActiveComponentType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public int SnapshotSize => UnsafeUtility.SizeOf<ProjectileSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeActiveComponent = ComponentType.ReadWrite<ActiveComponent>();
        componentTypeOwnerComponent = ComponentType.ReadWrite<OwnerComponent>();
        componentTypeProjectileTag = ComponentType.ReadWrite<ProjectileTag>();
        componentTypePhysicsCollider = ComponentType.ReadWrite<PhysicsCollider>();
        componentTypePhysicsGravityFactor = ComponentType.ReadWrite<PhysicsGravityFactor>();
        componentTypePhysicsMass = ComponentType.ReadWrite<PhysicsMass>();
        componentTypePhysicsVelocity = ComponentType.ReadWrite<PhysicsVelocity>();
        componentTypeCompositeScale = ComponentType.ReadWrite<CompositeScale>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostActiveComponentType = system.GetComponentTypeHandle<ActiveComponent>(true);
        ghostRotationType = system.GetComponentTypeHandle<Rotation>(true);
        ghostTranslationType = system.GetComponentTypeHandle<Translation>(true);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref ProjectileSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataActiveComponent = chunk.GetNativeArray(ghostActiveComponentType);
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetActiveComponentIsActive(chunkDataActiveComponent[ent].IsActive, serializerState);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
