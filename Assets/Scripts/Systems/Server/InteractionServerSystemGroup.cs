using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateAfter(typeof(PostTriggerEventServerSystem))]
[UpdateBefore(typeof(AlignObjectServerSystem))]
public class InteractionServerSystemGroup : ComponentSystemGroup
{
}
