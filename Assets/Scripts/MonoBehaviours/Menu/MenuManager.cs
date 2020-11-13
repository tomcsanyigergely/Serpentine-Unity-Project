using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMenu
{
    void Init();
    void Enter();
    void Exit();
}

public class MenuManager : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private JoinGameMenu joinGameMenu;
    [SerializeField] private JoinLanMenu joinLanMenu;
    [SerializeField] private JoinOnlineMenu joinOnlineMenu;
    [SerializeField] private HostGameMenu hostGameMenu;
    [SerializeField] private CreditsScreen creditsScreen;

    private Dictionary<Type, IMenu> menuDictonary = new Dictionary<Type, IMenu>();

    private IMenu selectedMenu;

    private void Awake()
    {
        GameSession.clientSession = null;
        GameSession.serverSession = null;

        selectedMenu = mainMenu;
    }

    private void Start()
    {
        menuDictonary.Add(mainMenu.GetType(), mainMenu);
        menuDictonary.Add(joinGameMenu.GetType(), joinGameMenu);
        menuDictonary.Add(joinLanMenu.GetType(), joinLanMenu);
        menuDictonary.Add(joinOnlineMenu.GetType(), joinOnlineMenu);
        menuDictonary.Add(hostGameMenu.GetType(), hostGameMenu);
        menuDictonary.Add(creditsScreen.GetType(), creditsScreen);

        foreach (var pair in menuDictonary)
        {
            pair.Value.Init();
            pair.Value.Exit();
        }
        
        selectedMenu.Enter();
    }

    public void SelectMenu<T>() where T : IMenu
    {
        selectedMenu.Exit();
        selectedMenu = menuDictonary[typeof(T)];
        selectedMenu.Enter();
    }
}
