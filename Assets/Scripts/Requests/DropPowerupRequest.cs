using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;

[BurstCompile]
public struct DropPowerupRequest : IRpcCommand
{
    public uint slotNumber;

    public void Deserialize(ref DataStreamReader reader)
    {
        slotNumber = reader.ReadUIntNetworkByteOrder();
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUIntNetworkByteOrder(slotNumber);
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<DropPowerupRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }    
}

public class DropPowerupRequestSystem : RpcCommandRequestSystem<DropPowerupRequest>
{
}