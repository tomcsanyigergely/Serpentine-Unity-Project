using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class JoinOnlineMenu : MonoBehaviour, IMenu
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private ServerConfiguration serverConfiguration;

    [SerializeField] private Canvas joinOnlineMenuCanvas;

    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button cancelButton;
    
    public void Init()
    {
        portInputField.placeholder.GetComponent<TextMeshProUGUI>().text = $"(default: {serverConfiguration.defaultServerPort})";
        
        connectButton.onClick.AddListener(() =>
        {
            try
            {
                if (IPAddress.TryParse(ipInputField.text, out var ipAddress))
                {

                    GameSession.serverSession = null;

                    GameSession.clientSession = new ClientSession
                    {
                        remoteServerIpAddress = ipInputField.text,
                        remoteServerPort = !portInputField.text.Equals("") ? Convert.ToUInt16(portInputField.text) : serverConfiguration.defaultServerPort
                    };

                    SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
                }
            }
            catch (Exception e)
            {
                
            }
        });
        
        cancelButton.onClick.AddListener(() =>
        {
            menuManager.SelectMenu<JoinGameMenu>();
        });
    }

    public void Enter()
    {
        ipInputField.text = "";
        portInputField.text = "";
        joinOnlineMenuCanvas.gameObject.SetActive(true);
    }

    public void Exit()
    {
        joinOnlineMenuCanvas.gameObject.SetActive(false);
    }
}
