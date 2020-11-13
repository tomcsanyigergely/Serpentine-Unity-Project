using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalPlayerHealthUpdater : MonoBehaviour
{
    [SerializeField] private ProgressBar localPlayerHealthBar;
    [SerializeField] private TextMeshProUGUI localPlayerHealthText;

    public void OnUpdateHealth(float health)
    {
        localPlayerHealthBar.SetProgression(health / SerializedFields.singleton.maxHealth);
        localPlayerHealthText.text = Mathf.CeilToInt(health).ToString();
    }
}
