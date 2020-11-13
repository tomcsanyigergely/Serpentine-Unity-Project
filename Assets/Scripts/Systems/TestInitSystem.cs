using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Events;

[UpdateInWorld(UpdateInWorld.TargetWorld.Default)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class TestInitSystem : ComponentSystem
{
    public Queue<Action> actions = new Queue<Action>();

    protected override void OnUpdate()
    {
        while (actions.Count > 0)
        {
            actions.Dequeue()();
        }
    }
}
