using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct SerpentineGhostSerializerCollection : IGhostSerializerCollection
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
    public static int FindGhostType<T>()
        where T : struct, ISnapshotData<T>
    {
        if (typeof(T) == typeof(CarStubSnapshotData))
            return 0;
        if (typeof(T) == typeof(ProjectileSnapshotData))
            return 1;
        if (typeof(T) == typeof(MissileSnapshotData))
            return 2;
        return -1;
    }

    public void BeginSerialize(ComponentSystemBase system)
    {
        m_CarStubGhostSerializer.BeginSerialize(system);
        m_ProjectileGhostSerializer.BeginSerialize(system);
        m_MissileGhostSerializer.BeginSerialize(system);
    }

    public int CalculateImportance(int serializer, ArchetypeChunk chunk)
    {
        switch (serializer)
        {
            case 0:
                return m_CarStubGhostSerializer.CalculateImportance(chunk);
            case 1:
                return m_ProjectileGhostSerializer.CalculateImportance(chunk);
            case 2:
                return m_MissileGhostSerializer.CalculateImportance(chunk);
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int GetSnapshotSize(int serializer)
    {
        switch (serializer)
        {
            case 0:
                return m_CarStubGhostSerializer.SnapshotSize;
            case 1:
                return m_ProjectileGhostSerializer.SnapshotSize;
            case 2:
                return m_MissileGhostSerializer.SnapshotSize;
        }

        throw new ArgumentException("Invalid serializer type");
    }

    public int Serialize(ref DataStreamWriter dataStream, SerializeData data)
    {
        switch (data.ghostType)
        {
            case 0:
            {
                return GhostSendSystem<SerpentineGhostSerializerCollection>.InvokeSerialize<CarStubGhostSerializer, CarStubSnapshotData>(m_CarStubGhostSerializer, ref dataStream, data);
            }
            case 1:
            {
                return GhostSendSystem<SerpentineGhostSerializerCollection>.InvokeSerialize<ProjectileGhostSerializer, ProjectileSnapshotData>(m_ProjectileGhostSerializer, ref dataStream, data);
            }
            case 2:
            {
                return GhostSendSystem<SerpentineGhostSerializerCollection>.InvokeSerialize<MissileGhostSerializer, MissileSnapshotData>(m_MissileGhostSerializer, ref dataStream, data);
            }
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    private CarStubGhostSerializer m_CarStubGhostSerializer;
    private ProjectileGhostSerializer m_ProjectileGhostSerializer;
    private MissileGhostSerializer m_MissileGhostSerializer;
}

public struct EnableSerpentineGhostSendSystemComponent : IComponentData
{}
public class SerpentineGhostSendSystem : GhostSendSystem<SerpentineGhostSerializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableSerpentineGhostSendSystemComponent>();
    }

    public override bool IsEnabled()
    {
        return HasSingleton<EnableSerpentineGhostSendSystemComponent>();
    }
}
