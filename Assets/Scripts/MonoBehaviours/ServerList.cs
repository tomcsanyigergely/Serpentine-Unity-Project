using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerList : MonoBehaviour
{
    [SerializeField] private GameObject serverListElementPrefab;

    [SerializeField] private TextMeshProUGUI hostedByColumnHeaderText;
    [SerializeField] private TextMeshProUGUI playersColumnHeaderText;
    [SerializeField] private TextMeshProUGUI lapsColumnHeaderText;

    [SerializeField] private LayoutElement hostedByColumnHeaderLayoutElement;
    [SerializeField] private LayoutElement playersColumnHeaderLayoutElement;
    [SerializeField] private LayoutElement lapsColumnHeaderLayoutElement;

    public List<ServerListElement> serverListElements = new List<ServerListElement>();

    private void Start()
    {
        UpdateColumnWidths();
    }

    public void Clear()
    {
        foreach (var element in serverListElements)
        {
            GameObject.Destroy(element.gameObject);
        }
        serverListElements.Clear();

        UpdateColumnWidths();
    }

    public void AddElement(DiscoveryResult discoveryResult)
    {
        ServerListElement serverListElement = GameObject.Instantiate(serverListElementPrefab, transform).GetComponent<ServerListElement>();
        serverListElements.Add(serverListElement);
        serverListElement.Init(discoveryResult);

        UpdateColumnWidths();
    }

    private void UpdateColumnWidths()
    {
        float hostedByColumnPreferredWidth = hostedByColumnHeaderText.preferredWidth;
        float playersColumnPreferredWidth = playersColumnHeaderText.preferredWidth;
        float lapsColumnPreferredWidth = lapsColumnHeaderText.preferredWidth;

        foreach (var serverListElement in serverListElements)
        {
            hostedByColumnPreferredWidth = Mathf.Max(hostedByColumnPreferredWidth, serverListElement.GetHostedByColumnTextPreferredWidth());
            playersColumnPreferredWidth = Mathf.Max(playersColumnPreferredWidth, serverListElement.GetPlayersColumnTextPreferredWidth());
            lapsColumnPreferredWidth = Mathf.Max(lapsColumnPreferredWidth, serverListElement.GetLapsColumnTextPreferredWidth());
        }

        hostedByColumnHeaderLayoutElement.preferredWidth = hostedByColumnPreferredWidth;
        playersColumnHeaderLayoutElement.preferredWidth = playersColumnPreferredWidth;
        lapsColumnHeaderLayoutElement.preferredWidth = lapsColumnPreferredWidth;

        foreach (var serverListElement in serverListElements)
        {
            serverListElement.SetLayoutPreferredWidths(hostedByColumnPreferredWidth, playersColumnPreferredWidth, lapsColumnPreferredWidth);
        }
    }
}
