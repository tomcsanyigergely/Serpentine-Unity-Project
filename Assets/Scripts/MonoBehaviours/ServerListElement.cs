using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ServerListElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hostedByColumnText;
    [SerializeField] private TextMeshProUGUI playersColumnText;
    [SerializeField] private TextMeshProUGUI lapsColumnText;
    
    [SerializeField] private LayoutElement hostedByColumnLayoutElement;
    [SerializeField] private LayoutElement playersColumnLayoutElement;
    [SerializeField] private LayoutElement lapsColumnLayoutElement;
    
    [SerializeField] private Button connectButton;

    public void Init(DiscoveryResult discoveryResult)
    {
        hostedByColumnText.text = discoveryResult.HostName;
        playersColumnText.text = discoveryResult.ConnectedPlayers + " / " + discoveryResult.NumberOfPlayers;
        lapsColumnText.text = discoveryResult.Laps.ToString();
        connectButton.onClick.AddListener(() =>
        {
            GameSession.serverSession = null;
            
            GameSession.clientSession = new ClientSession
            {
                remoteServerIpAddress = discoveryResult.RemoteServerIpAddress,
                remoteServerPort = discoveryResult.RemoteServerPort
            };

            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        });
    }

    public float GetHostedByColumnTextPreferredWidth()
    {
        return hostedByColumnText.preferredWidth;
    }

    public float GetPlayersColumnTextPreferredWidth()
    {
        return playersColumnText.preferredWidth;
    }

    public float GetLapsColumnTextPreferredWidth()
    {
        return lapsColumnText.preferredWidth;
    }

    public void SetLayoutPreferredWidths(float hostedByColumnWidth, float playersColumnWidth, float lapsColumnWidth)
    {
        hostedByColumnLayoutElement.preferredWidth = hostedByColumnWidth;
        playersColumnLayoutElement.preferredWidth = playersColumnWidth;
        lapsColumnLayoutElement.preferredWidth = lapsColumnWidth;
    }
}
