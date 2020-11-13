using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using System;

[BurstCompile]
public struct PlayerDiedRequest : IRpcCommand
{
    public int PlayerId;
    
    public void Deserialize(ref DataStreamReader reader)
    {
        PlayerId = reader.ReadIntNetworkByteOrder();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteIntNetworkByteOrder(PlayerId);
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<PlayerDiedRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

public class PlayerDiedRequestSystem : RpcCommandRequestSystem<PlayerDiedRequest>
{
}