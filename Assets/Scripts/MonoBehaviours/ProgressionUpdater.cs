using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class ProgressionUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lapsText;
    [SerializeField] private TextMeshProUGUI checkpointsText;

    private CheckpointInitializationSystem checkpointInitializationSystem;
    private RaceInformationClientSystem raceInformationClientSystem;

    private void Awake()
    {
        foreach (var world in World.All)
        {
            var progressionChangedClientSystem = world.GetExistingSystem<ProgressionUpdaterClientSystem>();
            if (progressionChangedClientSystem != null)
            {
                progressionChangedClientSystem.OnUpdatePlayerProgression += OnUpdatePlayerProgression;
            }

            var checkpointInitializationSystem = world.GetExistingSystem<CheckpointInitializationSystem>();
            if (checkpointInitializationSystem != null)
            {
                this.checkpointInitializationSystem = checkpointInitializationSystem;
            }

            var raceInformationClientSystem = world.GetExistingSystem<RaceInformationClientSystem>();
            if (raceInformationClientSystem != null)
            {
                this.raceInformationClientSystem = raceInformationClientSystem;
                this.raceInformationClientSystem.OnRaceInformationReceived += OnRaceInformationReceived;
            }

            var playerFinishedClientSystem = world.GetExistingSystem<PlayerFinishedClientSystem>();
            if (playerFinishedClientSystem != null)
            {
                playerFinishedClientSystem.OnPlayerFinished += OnPlayerFinished;
            }
        }
    }

    private void OnRaceInformationReceived()
    {
        OnUpdatePlayerProgression(0);
    }

    private void OnUpdatePlayerProgression(uint crossedCheckpoints)
    {
        lapsText.text = "Lap: " + ((crossedCheckpoints / checkpointInitializationSystem.numberOfCheckpoints) + 1) + " / " + raceInformationClientSystem.laps;
        checkpointsText.text = "CP: " + ((crossedCheckpoints % checkpointInitializationSystem.numberOfCheckpoints) + 1) + " / " + checkpointInitializationSystem.numberOfCheckpoints;
    }

    private void OnPlayerFinished(uint position)
    {
        lapsText.gameObject.SetActive(false);
        checkpointsText.gameObject.SetActive(false);
    }
}
