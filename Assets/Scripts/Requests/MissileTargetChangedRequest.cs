using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using System;

[BurstCompile]
public struct MissileTargetChangedRequest : IRpcCommand
{
    public bool HasTarget;
    public int TargetPlayerId;

    public void Deserialize(ref DataStreamReader reader)
    {
        HasTarget = Convert.ToBoolean(reader.ReadByte());
        if (HasTarget)
        {
            TargetPlayerId = reader.ReadIntNetworkByteOrder();
        }
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte(Convert.ToByte(HasTarget));
        if (HasTarget)
        {
            writer.WriteIntNetworkByteOrder(TargetPlayerId);
        }
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<MissileTargetChangedRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

public class MissileTargetChangedRequestSystem : RpcCommandRequestSystem<MissileTargetChangedRequest>
{
}