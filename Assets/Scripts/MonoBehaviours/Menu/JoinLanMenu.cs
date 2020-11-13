using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinLanMenu : MonoBehaviour, IMenu
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private ServerConfiguration serverConfiguration;
    [SerializeField] private NetworkDiscoverer networkDiscoverer;

    [SerializeField] private Canvas joinLanMenuCanvas;

    [SerializeField] private Button refreshButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private ServerList serverList;
    
    public void Init()
    {
        refreshButton.onClick.AddListener(RefreshServerList);
        
        cancelButton.onClick.AddListener(() =>
        {
            menuManager.SelectMenu<JoinGameMenu>();
        });

        networkDiscoverer.Init();
        networkDiscoverer.OnServerDiscovered += (DiscoveryResult discoveryResult) =>
        {
            serverList.AddElement(discoveryResult);
        };
    }

    public void Enter()
    {
        RefreshServerList();
        joinLanMenuCanvas.gameObject.SetActive(true);
    }

    public void Exit()
    {
        joinLanMenuCanvas.gameObject.SetActive(false);
    }

    private void RefreshServerList()
    {
        serverList.Clear();
        networkDiscoverer.Discover();
    }
}
