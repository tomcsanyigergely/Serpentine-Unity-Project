using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;

[UpdateInWorld(UpdateInWorld.TargetWorld.Client)]
[UpdateAfter(typeof(AfterSimulationInterpolationSystem))]
public class PlacementUpdateSystem : ComponentSystem
{
    public UnityAction<uint> OnUpdatePlacement;

    public uint numberOfPlayers = 0;
    
    private CheckpointInitializationSystem checkpointInitializationSystem;
    
    protected override void OnCreate()
    {
        checkpointInitializationSystem = World.GetOrCreateSystem<CheckpointInitializationSystem>();
    }

    private static int i = 0;

    protected override void OnUpdate()
    {
        if (i == 0 && HasSingleton<NetworkIdComponent>()) // Updating placement only every 3 ticks
        {
            uint playerPlacement = Utils.GetPlayerPlacement(GetSingleton<NetworkIdComponent>().Value, numberOfPlayers, EntityManager, Entities, this);
            OnUpdatePlacement?.Invoke(playerPlacement);
        }
        
        i = (i + 1) % 3;
    }

    
}
