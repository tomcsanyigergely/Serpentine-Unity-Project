using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public struct CarStubGhostSerializer : IGhostSerializer<CarStubSnapshotData>
{
    private ComponentType componentTypeBoostComponent;
    private ComponentType componentTypeCarComponent;
    private ComponentType componentTypeCarTag;
    private ComponentType componentTypeHealthComponent;
    private ComponentType componentTypeMissileScopeComponent;
    private ComponentType componentTypeProgressionComponent;
    private ComponentType componentTypeShieldComponent;
    private ComponentType componentTypeSynchronizedCarComponent;
    private ComponentType componentTypePhysicsCollider;
    private ComponentType componentTypePhysicsDamping;
    private ComponentType componentTypePhysicsMass;
    private ComponentType componentTypePhysicsVelocity;
    private ComponentType componentTypeCompositeScale;
    private ComponentType componentTypeLocalToWorld;
    private ComponentType componentTypeRotation;
    private ComponentType componentTypeTranslation;
    // FIXME: These disable safety since all serializers have an instance of the same type - causing aliasing. Should be fixed in a cleaner way
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<HealthComponent> ghostHealthComponentType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<ProgressionComponent> ghostProgressionComponentType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<SynchronizedCarComponent> ghostSynchronizedCarComponentType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<Rotation> ghostRotationType;
    [NativeDisableContainerSafetyRestriction][ReadOnly] private ComponentTypeHandle<Translation> ghostTranslationType;


    public int CalculateImportance(ArchetypeChunk chunk)
    {
        return 1;
    }

    public int SnapshotSize => UnsafeUtility.SizeOf<CarStubSnapshotData>();
    public void BeginSerialize(ComponentSystemBase system)
    {
        componentTypeBoostComponent = ComponentType.ReadWrite<BoostComponent>();
        componentTypeCarComponent = ComponentType.ReadWrite<CarComponent>();
        componentTypeCarTag = ComponentType.ReadWrite<CarTag>();
        componentTypeHealthComponent = ComponentType.ReadWrite<HealthComponent>();
        componentTypeMissileScopeComponent = ComponentType.ReadWrite<MissileScopeComponent>();
        componentTypeProgressionComponent = ComponentType.ReadWrite<ProgressionComponent>();
        componentTypeShieldComponent = ComponentType.ReadWrite<ShieldComponent>();
        componentTypeSynchronizedCarComponent = ComponentType.ReadWrite<SynchronizedCarComponent>();
        componentTypePhysicsCollider = ComponentType.ReadWrite<PhysicsCollider>();
        componentTypePhysicsDamping = ComponentType.ReadWrite<PhysicsDamping>();
        componentTypePhysicsMass = ComponentType.ReadWrite<PhysicsMass>();
        componentTypePhysicsVelocity = ComponentType.ReadWrite<PhysicsVelocity>();
        componentTypeCompositeScale = ComponentType.ReadWrite<CompositeScale>();
        componentTypeLocalToWorld = ComponentType.ReadWrite<LocalToWorld>();
        componentTypeRotation = ComponentType.ReadWrite<Rotation>();
        componentTypeTranslation = ComponentType.ReadWrite<Translation>();
        ghostHealthComponentType = system.GetComponentTypeHandle<HealthComponent>(true);
        ghostProgressionComponentType = system.GetComponentTypeHandle<ProgressionComponent>(true);
        ghostSynchronizedCarComponentType = system.GetComponentTypeHandle<SynchronizedCarComponent>(true);
        ghostRotationType = system.GetComponentTypeHandle<Rotation>(true);
        ghostTranslationType = system.GetComponentTypeHandle<Translation>(true);
    }

    public void CopyToSnapshot(ArchetypeChunk chunk, int ent, uint tick, ref CarStubSnapshotData snapshot, GhostSerializerState serializerState)
    {
        snapshot.tick = tick;
        var chunkDataHealthComponent = chunk.GetNativeArray(ghostHealthComponentType);
        var chunkDataProgressionComponent = chunk.GetNativeArray(ghostProgressionComponentType);
        var chunkDataSynchronizedCarComponent = chunk.GetNativeArray(ghostSynchronizedCarComponentType);
        var chunkDataRotation = chunk.GetNativeArray(ghostRotationType);
        var chunkDataTranslation = chunk.GetNativeArray(ghostTranslationType);
        snapshot.SetHealthComponentHealth(chunkDataHealthComponent[ent].Health, serializerState);
        snapshot.SetProgressionComponentCrossedCheckpoints(chunkDataProgressionComponent[ent].CrossedCheckpoints, serializerState);
        snapshot.SetSynchronizedCarComponentPlayerId(chunkDataSynchronizedCarComponent[ent].PlayerId, serializerState);
        snapshot.SetSynchronizedCarComponentSteerAngle(chunkDataSynchronizedCarComponent[ent].SteerAngle, serializerState);
        snapshot.SetSynchronizedCarComponentIsShieldActive(chunkDataSynchronizedCarComponent[ent].IsShieldActive, serializerState);
        snapshot.SetRotationValue(chunkDataRotation[ent].Value, serializerState);
        snapshot.SetTranslationValue(chunkDataTranslation[ent].Value, serializerState);
    }
}
