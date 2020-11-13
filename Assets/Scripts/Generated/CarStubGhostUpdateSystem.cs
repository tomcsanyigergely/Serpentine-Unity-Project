using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Networking.Transport.Utilities;
using Unity.NetCode;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(GhostUpdateSystemGroup))]
public class CarStubGhostUpdateSystem : JobComponentSystem
{
    [BurstCompile]
    struct UpdateInterpolatedJob : IJobChunk
    {
        [ReadOnly] public NativeHashMap<int, GhostEntity> GhostMap;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [NativeDisableContainerSafetyRestriction] public NativeArray<uint> minMaxSnapshotTick;
#pragma warning disable 649
        [NativeSetThreadIndex]
        public int ThreadIndex;
#pragma warning restore 649
#endif
        [ReadOnly] public BufferTypeHandle<CarStubSnapshotData> ghostSnapshotDataType;
        [ReadOnly] public EntityTypeHandle ghostEntityType;
        public ComponentTypeHandle<HealthComponent> ghostHealthComponentType;
        public ComponentTypeHandle<ProgressionComponent> ghostProgressionComponentType;
        public ComponentTypeHandle<SynchronizedCarComponent> ghostSynchronizedCarComponentType;
        public ComponentTypeHandle<Rotation> ghostRotationType;
        public ComponentTypeHandle<Translation> ghostTranslationType;

        public uint targetTick;
        public float targetTickFraction;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var deserializerState = new GhostDeserializerState
            {
                GhostMap = GhostMap
            };
            var ghostEntityArray = chunk.GetNativeArray(ghostEntityType);
            var ghostSnapshotDataArray = chunk.GetBufferAccessor(ghostSnapshotDataType);
            var ghostHealthComponentArray = chunk.GetNativeArray(ghostHealthComponentType);
            var ghostProgressionComponentArray = chunk.GetNativeArray(ghostProgressionComponentType);
            var ghostSynchronizedCarComponentArray = chunk.GetNativeArray(ghostSynchronizedCarComponentType);
            var ghostRotationArray = chunk.GetNativeArray(ghostRotationType);
            var ghostTranslationArray = chunk.GetNativeArray(ghostTranslationType);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var minMaxOffset = ThreadIndex * (JobsUtility.CacheLineSize/4);
#endif
            for (int entityIndex = 0; entityIndex < ghostEntityArray.Length; ++entityIndex)
            {
                var snapshot = ghostSnapshotDataArray[entityIndex];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                var latestTick = snapshot.GetLatestTick();
                if (latestTick != 0)
                {
                    if (minMaxSnapshotTick[minMaxOffset] == 0 || SequenceHelpers.IsNewer(minMaxSnapshotTick[minMaxOffset], latestTick))
                        minMaxSnapshotTick[minMaxOffset] = latestTick;
                    if (minMaxSnapshotTick[minMaxOffset + 1] == 0 || SequenceHelpers.IsNewer(latestTick, minMaxSnapshotTick[minMaxOffset + 1]))
                        minMaxSnapshotTick[minMaxOffset + 1] = latestTick;
                }
#endif
                // If there is no data found don't apply anything (would be default state), required for prespawned ghosts
                CarStubSnapshotData snapshotData;
                if (!snapshot.GetDataAtTick(targetTick, targetTickFraction, out snapshotData))
                    return;

                var ghostHealthComponent = ghostHealthComponentArray[entityIndex];
                var ghostProgressionComponent = ghostProgressionComponentArray[entityIndex];
                var ghostSynchronizedCarComponent = ghostSynchronizedCarComponentArray[entityIndex];
                var ghostRotation = ghostRotationArray[entityIndex];
                var ghostTranslation = ghostTranslationArray[entityIndex];
                ghostHealthComponent.Health = snapshotData.GetHealthComponentHealth(deserializerState);
                ghostProgressionComponent.CrossedCheckpoints = snapshotData.GetProgressionComponentCrossedCheckpoints(deserializerState);
                ghostSynchronizedCarComponent.PlayerId = snapshotData.GetSynchronizedCarComponentPlayerId(deserializerState);
                ghostSynchronizedCarComponent.SteerAngle = snapshotData.GetSynchronizedCarComponentSteerAngle(deserializerState);
                ghostSynchronizedCarComponent.IsShieldActive = snapshotData.GetSynchronizedCarComponentIsShieldActive(deserializerState);
                ghostRotation.Value = snapshotData.GetRotationValue(deserializerState);
                ghostTranslation.Value = snapshotData.GetTranslationValue(deserializerState);
                ghostHealthComponentArray[entityIndex] = ghostHealthComponent;
                ghostProgressionComponentArray[entityIndex] = ghostProgressionComponent;
                ghostSynchronizedCarComponentArray[entityIndex] = ghostSynchronizedCarComponent;
                ghostRotationArray[entityIndex] = ghostRotation;
                ghostTranslationArray[entityIndex] = ghostTranslation;
            }
        }
    }
    [BurstCompile]
    struct UpdatePredictedJob : IJobChunk
    {
        [ReadOnly] public NativeHashMap<int, GhostEntity> GhostMap;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [NativeDisableContainerSafetyRestriction] public NativeArray<uint> minMaxSnapshotTick;
#endif
#pragma warning disable 649
        [NativeSetThreadIndex]
        public int ThreadIndex;
#pragma warning restore 649
        [NativeDisableParallelForRestriction] public NativeArray<uint> minPredictedTick;
        [ReadOnly] public BufferTypeHandle<CarStubSnapshotData> ghostSnapshotDataType;
        [ReadOnly] public EntityTypeHandle ghostEntityType;
        public ComponentTypeHandle<PredictedGhostComponent> predictedGhostComponentType;
        public ComponentTypeHandle<HealthComponent> ghostHealthComponentType;
        public ComponentTypeHandle<ProgressionComponent> ghostProgressionComponentType;
        public ComponentTypeHandle<SynchronizedCarComponent> ghostSynchronizedCarComponentType;
        public ComponentTypeHandle<Rotation> ghostRotationType;
        public ComponentTypeHandle<Translation> ghostTranslationType;
        public uint targetTick;
        public uint lastPredictedTick;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var deserializerState = new GhostDeserializerState
            {
                GhostMap = GhostMap
            };
            var ghostEntityArray = chunk.GetNativeArray(ghostEntityType);
            var ghostSnapshotDataArray = chunk.GetBufferAccessor(ghostSnapshotDataType);
            var predictedGhostComponentArray = chunk.GetNativeArray(predictedGhostComponentType);
            var ghostHealthComponentArray = chunk.GetNativeArray(ghostHealthComponentType);
            var ghostProgressionComponentArray = chunk.GetNativeArray(ghostProgressionComponentType);
            var ghostSynchronizedCarComponentArray = chunk.GetNativeArray(ghostSynchronizedCarComponentType);
            var ghostRotationArray = chunk.GetNativeArray(ghostRotationType);
            var ghostTranslationArray = chunk.GetNativeArray(ghostTranslationType);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var minMaxOffset = ThreadIndex * (JobsUtility.CacheLineSize/4);
#endif
            for (int entityIndex = 0; entityIndex < ghostEntityArray.Length; ++entityIndex)
            {
                var snapshot = ghostSnapshotDataArray[entityIndex];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                var latestTick = snapshot.GetLatestTick();
                if (latestTick != 0)
                {
                    if (minMaxSnapshotTick[minMaxOffset] == 0 || SequenceHelpers.IsNewer(minMaxSnapshotTick[minMaxOffset], latestTick))
                        minMaxSnapshotTick[minMaxOffset] = latestTick;
                    if (minMaxSnapshotTick[minMaxOffset + 1] == 0 || SequenceHelpers.IsNewer(latestTick, minMaxSnapshotTick[minMaxOffset + 1]))
                        minMaxSnapshotTick[minMaxOffset + 1] = latestTick;
                }
#endif
                CarStubSnapshotData snapshotData;
                snapshot.GetDataAtTick(targetTick, out snapshotData);

                var predictedData = predictedGhostComponentArray[entityIndex];
                var lastPredictedTickInst = lastPredictedTick;
                if (lastPredictedTickInst == 0 || predictedData.AppliedTick != snapshotData.Tick)
                    lastPredictedTickInst = snapshotData.Tick;
                else if (!SequenceHelpers.IsNewer(lastPredictedTickInst, snapshotData.Tick))
                    lastPredictedTickInst = snapshotData.Tick;
                if (minPredictedTick[ThreadIndex] == 0 || SequenceHelpers.IsNewer(minPredictedTick[ThreadIndex], lastPredictedTickInst))
                    minPredictedTick[ThreadIndex] = lastPredictedTickInst;
                predictedGhostComponentArray[entityIndex] = new PredictedGhostComponent{AppliedTick = snapshotData.Tick, PredictionStartTick = lastPredictedTickInst};
                if (lastPredictedTickInst != snapshotData.Tick)
                    continue;

                var ghostHealthComponent = ghostHealthComponentArray[entityIndex];
                var ghostProgressionComponent = ghostProgressionComponentArray[entityIndex];
                var ghostSynchronizedCarComponent = ghostSynchronizedCarComponentArray[entityIndex];
                var ghostRotation = ghostRotationArray[entityIndex];
                var ghostTranslation = ghostTranslationArray[entityIndex];
                ghostHealthComponent.Health = snapshotData.GetHealthComponentHealth(deserializerState);
                ghostProgressionComponent.CrossedCheckpoints = snapshotData.GetProgressionComponentCrossedCheckpoints(deserializerState);
                ghostSynchronizedCarComponent.PlayerId = snapshotData.GetSynchronizedCarComponentPlayerId(deserializerState);
                ghostSynchronizedCarComponent.SteerAngle = snapshotData.GetSynchronizedCarComponentSteerAngle(deserializerState);
                ghostSynchronizedCarComponent.IsShieldActive = snapshotData.GetSynchronizedCarComponentIsShieldActive(deserializerState);
                ghostRotation.Value = snapshotData.GetRotationValue(deserializerState);
                ghostTranslation.Value = snapshotData.GetTranslationValue(deserializerState);
                ghostHealthComponentArray[entityIndex] = ghostHealthComponent;
                ghostProgressionComponentArray[entityIndex] = ghostProgressionComponent;
                ghostSynchronizedCarComponentArray[entityIndex] = ghostSynchronizedCarComponent;
                ghostRotationArray[entityIndex] = ghostRotation;
                ghostTranslationArray[entityIndex] = ghostTranslation;
            }
        }
    }
    private ClientSimulationSystemGroup m_ClientSimulationSystemGroup;
    private GhostPredictionSystemGroup m_GhostPredictionSystemGroup;
    private EntityQuery m_interpolatedQuery;
    private EntityQuery m_predictedQuery;
    private GhostUpdateSystemGroup m_GhostUpdateSystemGroup;
    private uint m_LastPredictedTick;
    protected override void OnCreate()
    {
        m_GhostUpdateSystemGroup = World.GetOrCreateSystem<GhostUpdateSystemGroup>();
        m_ClientSimulationSystemGroup = World.GetOrCreateSystem<ClientSimulationSystemGroup>();
        m_GhostPredictionSystemGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();
        m_interpolatedQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []{
                ComponentType.ReadWrite<CarStubSnapshotData>(),
                ComponentType.ReadOnly<GhostComponent>(),
                ComponentType.ReadWrite<HealthComponent>(),
                ComponentType.ReadWrite<ProgressionComponent>(),
                ComponentType.ReadWrite<SynchronizedCarComponent>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadWrite<Translation>(),
            },
            None = new []{ComponentType.ReadWrite<PredictedGhostComponent>()}
        });
        m_predictedQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new []{
                ComponentType.ReadOnly<CarStubSnapshotData>(),
                ComponentType.ReadOnly<GhostComponent>(),
                ComponentType.ReadOnly<PredictedGhostComponent>(),
                ComponentType.ReadWrite<HealthComponent>(),
                ComponentType.ReadWrite<ProgressionComponent>(),
                ComponentType.ReadWrite<SynchronizedCarComponent>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadWrite<Translation>(),
            }
        });
        RequireForUpdate(GetEntityQuery(ComponentType.ReadWrite<CarStubSnapshotData>(),
            ComponentType.ReadOnly<GhostComponent>()));
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ghostEntityMap = m_GhostUpdateSystemGroup.GhostEntityMap;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var ghostMinMaxSnapshotTick = m_GhostUpdateSystemGroup.GhostSnapshotTickMinMax;
#endif
        if (!m_predictedQuery.IsEmptyIgnoreFilter)
        {
            var updatePredictedJob = new UpdatePredictedJob
            {
                GhostMap = ghostEntityMap,
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                minMaxSnapshotTick = ghostMinMaxSnapshotTick,
#endif
                minPredictedTick = m_GhostPredictionSystemGroup.OldestPredictedTick,
                ghostSnapshotDataType = GetBufferTypeHandle<CarStubSnapshotData>(true),
                ghostEntityType = GetEntityTypeHandle(),
                predictedGhostComponentType = GetComponentTypeHandle<PredictedGhostComponent>(),
                ghostHealthComponentType = GetComponentTypeHandle<HealthComponent>(),
                ghostProgressionComponentType = GetComponentTypeHandle<ProgressionComponent>(),
                ghostSynchronizedCarComponentType = GetComponentTypeHandle<SynchronizedCarComponent>(),
                ghostRotationType = GetComponentTypeHandle<Rotation>(),
                ghostTranslationType = GetComponentTypeHandle<Translation>(),

                targetTick = m_ClientSimulationSystemGroup.ServerTick,
                lastPredictedTick = m_LastPredictedTick
            };
            m_LastPredictedTick = m_ClientSimulationSystemGroup.ServerTick;
            if (m_ClientSimulationSystemGroup.ServerTickFraction < 1)
                m_LastPredictedTick = 0;
            inputDeps = updatePredictedJob.Schedule(m_predictedQuery, JobHandle.CombineDependencies(inputDeps, m_GhostUpdateSystemGroup.LastGhostMapWriter));
            m_GhostPredictionSystemGroup.AddPredictedTickWriter(inputDeps);
        }
        if (!m_interpolatedQuery.IsEmptyIgnoreFilter)
        {
            var updateInterpolatedJob = new UpdateInterpolatedJob
            {
                GhostMap = ghostEntityMap,
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                minMaxSnapshotTick = ghostMinMaxSnapshotTick,
#endif
                ghostSnapshotDataType = GetBufferTypeHandle<CarStubSnapshotData>(true),
                ghostEntityType = GetEntityTypeHandle(),
                ghostHealthComponentType = GetComponentTypeHandle<HealthComponent>(),
                ghostProgressionComponentType = GetComponentTypeHandle<ProgressionComponent>(),
                ghostSynchronizedCarComponentType = GetComponentTypeHandle<SynchronizedCarComponent>(),
                ghostRotationType = GetComponentTypeHandle<Rotation>(),
                ghostTranslationType = GetComponentTypeHandle<Translation>(),
                targetTick = m_ClientSimulationSystemGroup.InterpolationTick,
                targetTickFraction = m_ClientSimulationSystemGroup.InterpolationTickFraction
            };
            inputDeps = updateInterpolatedJob.Schedule(m_interpolatedQuery, JobHandle.CombineDependencies(inputDeps, m_GhostUpdateSystemGroup.LastGhostMapWriter));
        }
        return inputDeps;
    }
}
public partial class CarStubGhostSpawnSystem : DefaultGhostSpawnSystem<CarStubSnapshotData>
{
}
