using System;
using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class ResurrectionCountdownUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deathCountdownText;
    
    private WaitForSeconds waitOneSecond = new WaitForSeconds(1.0f);
    private Coroutine coroutine;
    private bool coroutineIsRunning = false;

    private void Awake()
    {
        deathCountdownText.text = "";
        
        foreach (var world in World.All)
        {
            var playerDiedRequestClientSystem = world.GetExistingSystem<PlayerDiedRequestClientSystem>();
            if (playerDiedRequestClientSystem != null)
            {
                playerDiedRequestClientSystem.OnPlayerDied += OnPlayerDied;
                return;
            }
        }
    }

    private void OnPlayerDied()
    {
        if (coroutineIsRunning)
        {
            StopCoroutine(coroutine);
        }

        coroutineIsRunning = true;
        coroutine = StartCoroutine(DisplayCountdown(SerializedFields.singleton.deathPenaltyInSeconds));
    }

    // unused, but might be useful:
    private void OnPlayerResurrected()
    {
        if (coroutineIsRunning)
        {
            coroutineIsRunning = false;
            StopCoroutine(coroutine);
            deathCountdownText.text = "";
        }
    }

    private IEnumerator DisplayCountdown(int seconds)
    {
        while (seconds > 0)
        {
            deathCountdownText.text = "Resurrection in\n" + seconds + "\nseconds";
            yield return waitOneSecond;
            seconds--;
        }
        deathCountdownText.text = "";
        coroutineIsRunning = false;
    }
}
