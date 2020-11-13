using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class InputActionHandlerClientSystem : EntityCommandBufferSystem
{
    private EntityCommandBuffer commandBuffer;
    private bool commandBufferAllocated = false;

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        commandBufferAllocated = false;
        // commandBuffer will be deallocated automatically after the execution
        // of this method, in EntityCommandBufferSystem.FlushPendingBuffers() called by EntityCommandBufferSystem.OnUpdate().
        // We have to set the commandBufferAllocated variable to false, to make sure a new commandBuffer is allocated when an input action is queued in the next frame.
    }

    public void QueueUseSlotAction(uint slotIndex)
    {
        if (slotIndex < SerializedFields.singleton.numberOfPowerupSlots)
        {
            AllocateCommandBuffer();
            
            var usePowerupRequest = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(usePowerupRequest, new UsePowerupRequest { slotNumber = slotIndex });
            commandBuffer.AddComponent(usePowerupRequest, new SendRpcCommandRequestComponent { TargetConnection = GetSingletonEntity<NetworkIdComponent>() });
        }
    }

    public void QueueDropSlotAction(uint slotIndex)
    {
        if (slotIndex < SerializedFields.singleton.numberOfPowerupSlots)
        {
            AllocateCommandBuffer();
            
            var dropPowerupRequest = commandBuffer.CreateEntity();
            commandBuffer.AddComponent(dropPowerupRequest, new DropPowerupRequest { slotNumber = slotIndex });
            commandBuffer.AddComponent(dropPowerupRequest, new SendRpcCommandRequestComponent { TargetConnection = GetSingletonEntity<NetworkIdComponent>() });
        }
    }

    private void AllocateCommandBuffer()
    {
        if (commandBufferAllocated == false)
        {
            commandBuffer = CreateCommandBuffer();
            commandBufferAllocated = true;
        }
    }
}
