using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiscoverButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private NetworkDiscoverer networkDiscoverer;
    
    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            networkDiscoverer.Discover();
        });
    }
}
