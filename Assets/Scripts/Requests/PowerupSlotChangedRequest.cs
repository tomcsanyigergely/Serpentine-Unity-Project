using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using System;

[BurstCompile]
public unsafe struct PowerupSlotChangedRequest : IRpcCommand
{
    public interface PowerupSlotData { }

    public struct LaserPowerupSlotData : PowerupSlotData
    {
        public uint RemainingShots;
    }

    public uint SlotNumber;
    public PowerupSlotContent SlotContent;
    public void* SlotData;

    public void Deserialize(ref DataStreamReader reader)
    {
        SlotNumber = reader.ReadUIntNetworkByteOrder();
        SlotContent = (PowerupSlotContent)reader.ReadUIntNetworkByteOrder();
        switch(SlotContent)
        {            
            case PowerupSlotContent.Laser:
                LaserPowerupSlotData* slotData = (LaserPowerupSlotData*)UnsafeUtility.Malloc(sizeof(LaserPowerupSlotElement), sizeof(LaserPowerupSlotElement), Unity.Collections.Allocator.Persistent);
                slotData->RemainingShots = reader.ReadUIntNetworkByteOrder();
                SlotData = slotData;
                break;
        }
    }

    public void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteUIntNetworkByteOrder(SlotNumber);
        writer.WriteUIntNetworkByteOrder((uint)SlotContent);
        switch(SlotContent)
        {
            case PowerupSlotContent.Laser:
                LaserPowerupSlotData* slotData = (LaserPowerupSlotData*)SlotData;
                writer.WriteUIntNetworkByteOrder(slotData->RemainingShots);                
                break;
        }

        if (SlotData != null)
        {
            UnsafeUtility.Free(SlotData, Unity.Collections.Allocator.Persistent);
        }
    }

    [BurstCompile]
    private static void InvokeExecute(ref RpcExecutor.Parameters parameters)
    {
        RpcExecutor.ExecuteCreateRequestComponent<PowerupSlotChangedRequest>(ref parameters);
    }

    static PortableFunctionPointer<RpcExecutor.ExecuteDelegate> InvokeExecuteFunctionPointer =
        new PortableFunctionPointer<RpcExecutor.ExecuteDelegate>(InvokeExecute);

    public PortableFunctionPointer<RpcExecutor.ExecuteDelegate> CompileExecute()
    {
        return InvokeExecuteFunctionPointer;
    }
}

public class PowerupSlotChangedRequestSystem : RpcCommandRequestSystem<PowerupSlotChangedRequest>
{
}