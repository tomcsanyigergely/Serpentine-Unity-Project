using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class FinishedMessageUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI finishedMessageText;

    private void Awake()
    {
        finishedMessageText.text = "";

        foreach (var world in World.All)
        {
            var playerFinishedClientSystem = world.GetExistingSystem<PlayerFinishedClientSystem>();
            if (playerFinishedClientSystem != null)
            {
                playerFinishedClientSystem.OnPlayerFinished += OnPlayerFinished;
            }
        }
    }

    private void OnPlayerFinished(uint position)
    {
        string suffix =
            (position % 10) == 1 ? "st" :
            (position % 10) == 2 ? "nd" :
            (position % 10) == 3 ? "rd" : 
            "th";
        
        finishedMessageText.text = "Finished!\n" + position.ToString() + suffix;
    }
}
