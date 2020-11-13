using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using System;

[BurstCompile]
public struct PlayerFinishedRequest : IRpcCommand
{
    public uint Position;

    public void Deserialize(ref DataStreamReader reader)
    {
        Position = reader.ReadUIntNetworkByteOrder();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUIntNetworkByteOrder(Position);
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<PlayerFinishedRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

public class PlayerFinishedRequestSystem : RpcCommandRequestSystem<PlayerFinishedRequest>
{
}