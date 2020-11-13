using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class StartCountdownUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startCountdownText;
    
    private WaitForSeconds waitOneSecond = new WaitForSeconds(1.0f);
    private Coroutine coroutine;
    private bool coroutineIsRunning = false;

    private void Awake()
    {
        startCountdownText.text = "Waiting for others...";
        
        foreach (var world in World.All)
        {
            var countdownStartedClientSystem = world.GetExistingSystem<CountdownStartedClientSystem>();
            if (countdownStartedClientSystem != null)
            {
                countdownStartedClientSystem.OnCountdownStarted += OnCountdownStarted;
            }

            var raceStartedClientSystem = world.GetExistingSystem<RaceStartedClientSystem>();
            if (raceStartedClientSystem != null)
            {
                raceStartedClientSystem.OnRaceStarted += OnRaceStarted;
            }
        }
    }

    private void OnCountdownStarted(uint countdownSeconds)
    {
        if (coroutineIsRunning)
        {
            StopCoroutine(coroutine);
        }

        coroutineIsRunning = true;
        coroutine = StartCoroutine(DisplayCountdown(countdownSeconds));
    }

    private void OnRaceStarted()
    {
        if (coroutineIsRunning)
        {
            coroutineIsRunning = false;
            StopCoroutine(coroutine);
        }

        coroutine = StartCoroutine(DisplayGo());
    }

    private IEnumerator DisplayCountdown(uint seconds)
    {
        while (seconds > 0)
        {
            startCountdownText.text = seconds.ToString();
            yield return waitOneSecond;
            seconds--;
        }
        coroutineIsRunning = false;
    }

    private IEnumerator DisplayGo()
    {
        startCountdownText.text = "GO!";
        yield return waitOneSecond;
        yield return waitOneSecond;
        startCountdownText.text = "";
        coroutineIsRunning = false;
    }
}
