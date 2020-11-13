using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class GameRulesPanel : MonoBehaviour
{
    [SerializeField] private GameObject gameRulesPanel;
    
    [SerializeField] private GameObject gameRulesPage;
    [SerializeField] private GameObject powerupsPage;

    [SerializeField] private Button powerupsButton;
    [SerializeField] private Button gameRulesButton;
    [SerializeField] private Button readyButton;

    private PlayerReadyClientSystem playerReadyClientSystem;
    
    private void Awake()
    {
        foreach (var world in World.All)
        {
            var playerReadyClientSystem = world.GetExistingSystem<PlayerReadyClientSystem>();
            if (playerReadyClientSystem != null)
            {
                this.playerReadyClientSystem = playerReadyClientSystem;
            }
        }
    }
    
    private void Start()
    {
        gameRulesPanel.gameObject.SetActive(true);
        gameRulesPage.gameObject.SetActive(true);
        powerupsPage.gameObject.SetActive(false);
        
        powerupsButton.onClick.AddListener(() =>
        {
            gameRulesPage.gameObject.SetActive(false);
            powerupsPage.gameObject.SetActive(true);
        });
        
        gameRulesButton.onClick.AddListener(() =>
        {
            powerupsPage.gameObject.SetActive(false);
            gameRulesPage.gameObject.SetActive(true);
        });
        
        readyButton.onClick.AddListener(() =>
        {
            gameRulesPanel.gameObject.SetActive(false);
            playerReadyClientSystem.SendPlayerReadyRequest();
        });
    }
}
