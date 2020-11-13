using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScreen : MonoBehaviour, IMenu
{
    [SerializeField] private MenuManager menuManager;
    
    [SerializeField] private Canvas creditsScreen;
    
    [SerializeField] private Button backToMainMenuButton;
    
    public void Init()
    {
        backToMainMenuButton.onClick.AddListener(() =>
        {
            menuManager.SelectMenu<MainMenu>();
        });
    }

    public void Enter()
    {
        creditsScreen.gameObject.SetActive(true);
    }

    public void Exit()
    {
        creditsScreen.gameObject.SetActive(false);
    }
}
