using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[UpdateInGroup(typeof(ClientSimulationSystemGroup))]
public class ProgressionUpdaterClientSystem : ComponentSystem
{
    public UnityAction<uint> OnUpdatePlayerProgression;
    
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity carEntity, ref SynchronizedCarComponent synchronizedCarComponent, ref ProgressionComponent progressionComponent) =>
        {
            var carPlayerId = synchronizedCarComponent.PlayerId;

            if (carPlayerId == GetSingleton<NetworkIdComponent>().Value)
            {
                OnUpdatePlayerProgression?.Invoke(progressionComponent.CrossedCheckpoints);
            }
        });
    }
}
