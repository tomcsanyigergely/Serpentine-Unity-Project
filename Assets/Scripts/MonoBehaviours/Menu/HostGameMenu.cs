using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HostGameMenu : MonoBehaviour, IMenu
{
    [SerializeField] private MenuManager menuManager;
    [SerializeField] private ServerConfiguration serverConfiguration;

    [SerializeField] private Canvas hostGameMenuCanvas;

    [SerializeField] private TMP_InputField hostNameInputField;
    [SerializeField] private TMP_InputField numberOfPlayersInputField;
    [SerializeField] private TMP_InputField lapsInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private Button startButton;
    [SerializeField] private Button cancelButton;

    public void Init()
    {
        hostNameInputField.placeholder.GetComponent<TextMeshProUGUI>().text = $"(default: {serverConfiguration.defaultHostName})";
        numberOfPlayersInputField.placeholder.GetComponent<TextMeshProUGUI>().text = $"(default: {serverConfiguration.defaultNumberOfPlayers}, max: 8)";
        lapsInputField.placeholder.GetComponent<TextMeshProUGUI>().text = $"(default: {serverConfiguration.defaultLaps})";
        portInputField.placeholder.GetComponent<TextMeshProUGUI>().text = $"(default: {serverConfiguration.defaultServerPort})";
        
        startButton.onClick.AddListener(() =>
        {
            try
            {
                uint numberOfPlayers = !numberOfPlayersInputField.text.Equals("") ? Convert.ToUInt32(numberOfPlayersInputField.text) : serverConfiguration.defaultNumberOfPlayers;
                uint laps = !lapsInputField.text.Equals("") ? Convert.ToUInt32(lapsInputField.text) : serverConfiguration.defaultLaps;

                if (laps >= 1 && laps <= 1000 && numberOfPlayers >= 1 && numberOfPlayers <= 8)
                {
                    GameSession.serverSession = new ServerSession
                    {
                        hostName = !hostNameInputField.text.Equals("") ? hostNameInputField.text : serverConfiguration.defaultHostName,
                        numberOfPlayers = numberOfPlayers,
                        laps = laps,
                        serverPort = !portInputField.text.Equals("") ? Convert.ToUInt16(portInputField.text) : serverConfiguration.defaultServerPort
                    };

                    GameSession.clientSession = new ClientSession
                    {
                        remoteServerIpAddress = "127.0.0.1",
                        remoteServerPort = GameSession.serverSession.serverPort
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
            menuManager.SelectMenu<MainMenu>();
        });
    }

    public void Enter()
    {
        hostNameInputField.text = "";
        numberOfPlayersInputField.text = "";
        lapsInputField.text = "";
        portInputField.text = "";
        
        hostGameMenuCanvas.gameObject.SetActive(true);
    }

    public void Exit()
    {
        hostGameMenuCanvas.gameObject.SetActive(false);
    }
}
