using Unity.Burst;
using Unity.NetCode;
using Unity.Networking.Transport;

[BurstCompile]
public struct RaceInformationRequest : IRpcCommand
{
    public uint laps;

    public void Deserialize(ref DataStreamReader reader)
    {
        laps = reader.ReadUIntNetworkByteOrder();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUIntNetworkByteOrder(laps);
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<RaceInformationRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

public class RaceInformationRequestSystem : RpcCommandRequestSystem<RaceInformationRequest>
{
}
