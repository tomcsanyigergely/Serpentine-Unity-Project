using Unity.Burst;
using Unity.NetCode;
using Unity.Networking.Transport;

[BurstCompile]
public struct CountdownStartedRequest : IRpcCommand
{
    public uint CountdownSeconds;
    
    public void Deserialize(ref DataStreamReader reader)
    {
        CountdownSeconds = reader.ReadUIntNetworkByteOrder();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUIntNetworkByteOrder(CountdownSeconds);
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<CountdownStartedRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

public class CountdownStartedRequestSystem : RpcCommandRequestSystem<CountdownStartedRequest>
{
}