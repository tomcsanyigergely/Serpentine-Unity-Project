using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;

[BurstCompile]
public struct UsePowerupRequest : IRpcCommand
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
        RpcExecutor.ExecuteCreateRequestComponent<UsePowerupRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }    
}

public class UsePowerupRequestSystem : RpcCommandRequestSystem<UsePowerupRequest>
{
}