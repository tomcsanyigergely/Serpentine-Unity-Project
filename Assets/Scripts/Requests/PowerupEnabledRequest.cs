using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using System;

[BurstCompile]
public struct PowerupEnabledRequest : IRpcCommand
{
    public uint PowerupId;
    public bool Enabled;

    public void Deserialize(ref DataStreamReader reader)
    {
        PowerupId = reader.ReadUIntNetworkByteOrder();
        Enabled = Convert.ToBoolean(reader.ReadByte());
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUIntNetworkByteOrder(PowerupId);
        writer.WriteByte(Convert.ToByte(Enabled));
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<PowerupEnabledRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

public class PowerupEnabledRequestSystem : RpcCommandRequestSystem<PowerupEnabledRequest>
{
}