using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class PlacementUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI positionText;

    private PlacementUpdateSystem placementUpdateSystem;

    private void Awake()
    {
        foreach (var world in World.All)
        {
            var placementUpdateSystem = world.GetExistingSystem<PlacementUpdateSystem>();
            if (placementUpdateSystem != null)
            {
                this.placementUpdateSystem = placementUpdateSystem;
                this.placementUpdateSystem.OnUpdatePlacement += OnUpdatePlacement;
            }

            var playerFinishedClientSystem = world.GetExistingSystem<PlayerFinishedClientSystem>();
            if (playerFinishedClientSystem != null)
            {
                playerFinishedClientSystem.OnPlayerFinished += OnPlayerFinished;
            }
        }
    }

    private void OnUpdatePlacement(uint placement)
    {
        positionText.text = "Pos: " + placement + " / " + placementUpdateSystem.numberOfPlayers;
    }

    private void OnPlayerFinished(uint position)
    {
        positionText.gameObject.SetActive(false);
    }
}
