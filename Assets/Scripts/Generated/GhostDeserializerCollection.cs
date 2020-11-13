using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct SerpentineGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "CarStubGhostSerializer",
            "ProjectileGhostSerializer",
            "MissileGhostSerializer",
        };
        return arr;
    }

    public int Length => 3;
#endif
    public void Initialize(World world)
    {
        var curCarStubGhostSpawnSystem = world.GetOrCreateSystem<CarStubGhostSpawnSystem>();
        m_CarStubSnapshotDataNewGhostIds = curCarStubGhostSpawnSystem.NewGhostIds;
        m_CarStubSnapshotDataNewGhosts = curCarStubGhostSpawnSystem.NewGhosts;
        curCarStubGhostSpawnSystem.GhostType = 0;
        var curProjectileGhostSpawnSystem = world.GetOrCreateSystem<ProjectileGhostSpawnSystem>();
        m_ProjectileSnapshotDataNewGhostIds = curProjectileGhostSpawnSystem.NewGhostIds;
        m_ProjectileSnapshotDataNewGhosts = curProjectileGhostSpawnSystem.NewGhosts;
        curProjectileGhostSpawnSystem.GhostType = 1;
        var curMissileGhostSpawnSystem = world.GetOrCreateSystem<MissileGhostSpawnSystem>();
        m_MissileSnapshotDataNewGhostIds = curMissileGhostSpawnSystem.NewGhostIds;
        m_MissileSnapshotDataNewGhosts = curMissileGhostSpawnSystem.NewGhosts;
        curMissileGhostSpawnSystem.GhostType = 2;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_CarStubSnapshotDataFromEntity = system.GetBufferFromEntity<CarStubSnapshotData>();
        m_ProjectileSnapshotDataFromEntity = system.GetBufferFromEntity<ProjectileSnapshotData>();
        m_MissileSnapshotDataFromEntity = system.GetBufferFromEntity<MissileSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<SerpentineGhostDeserializerCollection>.InvokeDeserialize(m_CarStubSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            case 1:
                return GhostReceiveSystem<SerpentineGhostDeserializerCollection>.InvokeDeserialize(m_ProjectileSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            case 2:
                return GhostReceiveSystem<SerpentineGhostDeserializerCollection>.InvokeDeserialize(m_MissileSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_CarStubSnapshotDataNewGhostIds.Add(ghostId);
                m_CarStubSnapshotDataNewGhosts.Add(GhostReceiveSystem<SerpentineGhostDeserializerCollection>.InvokeSpawn<CarStubSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            case 1:
                m_ProjectileSnapshotDataNewGhostIds.Add(ghostId);
                m_ProjectileSnapshotDataNewGhosts.Add(GhostReceiveSystem<SerpentineGhostDeserializerCollection>.InvokeSpawn<ProjectileSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            case 2:
                m_MissileSnapshotDataNewGhostIds.Add(ghostId);
                m_MissileSnapshotDataNewGhosts.Add(GhostReceiveSystem<SerpentineGhostDeserializerCollection>.InvokeSpawn<MissileSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<CarStubSnapshotData> m_CarStubSnapshotDataFromEntity;
    private NativeList<int> m_CarStubSnapshotDataNewGhostIds;
    private NativeList<CarStubSnapshotData> m_CarStubSnapshotDataNewGhosts;
    private BufferFromEntity<ProjectileSnapshotData> m_ProjectileSnapshotDataFromEntity;
    private NativeList<int> m_ProjectileSnapshotDataNewGhostIds;
    private NativeList<ProjectileSnapshotData> m_ProjectileSnapshotDataNewGhosts;
    private BufferFromEntity<MissileSnapshotData> m_MissileSnapshotDataFromEntity;
    private NativeList<int> m_MissileSnapshotDataNewGhostIds;
    private NativeList<MissileSnapshotData> m_MissileSnapshotDataNewGhosts;
}
public struct EnableSerpentineGhostReceiveSystemComponent : IComponentData
{}
public class SerpentineGhostReceiveSystem : GhostReceiveSystem<SerpentineGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableSerpentineGhostReceiveSystemComponent>();
    }
}
