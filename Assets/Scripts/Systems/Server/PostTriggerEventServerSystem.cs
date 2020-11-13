using Unity.Entities;
using Unity.NetCode;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateAfter(typeof(TriggerEventServerSystem))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public class PostTriggerEventServerSystem : EntityCommandBufferSystem
{    
}
