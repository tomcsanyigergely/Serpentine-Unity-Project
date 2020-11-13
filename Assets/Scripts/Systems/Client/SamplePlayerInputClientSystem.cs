using System;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class SamplePlayerInputClientSystem : ComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<NetworkIdComponent>();
        RequireSingletonForUpdate<EnableSerpentineGhostReceiveSystemComponent>();
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            Entities.WithNone<PlayerInput>().ForEach((Entity ent, ref SynchronizedCarComponent car) =>
            {
                if (car.PlayerId == localPlayerId)
                {
                    PostUpdateCommands.AddBuffer<PlayerInput>(ent);
                    PostUpdateCommands.SetComponent(GetSingletonEntity<CommandTargetComponent>(), new CommandTargetComponent { targetEntity = ent });
                }
            });
            return;
        }
        var input = default(PlayerInput);
        input.tick = World.GetExistingSystem<ClientSimulationSystemGroup>().ServerTick;

        input.keyForwardPressed = Convert.ToByte(Input.GetKey(SerializedFields.singleton.keyForward));
        input.keyBackwardPressed = Convert.ToByte(Input.GetKey(SerializedFields.singleton.keyBackward));
        input.keyTurnRightPressed = Convert.ToByte(Input.GetKey(SerializedFields.singleton.keyTurnRight));
        input.keyTurnLeftPressed = Convert.ToByte(Input.GetKey(SerializedFields.singleton.keyTurnLeft));

        var inputBuffer = EntityManager.GetBuffer<PlayerInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}