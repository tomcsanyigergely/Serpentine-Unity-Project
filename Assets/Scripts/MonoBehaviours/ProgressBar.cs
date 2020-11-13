using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private GameObject spriteMask;

    public void SetProgression(float progression)
    {
        spriteMask.transform.localScale = new Vector3(progression, 1, 1);
        spriteMask.transform.localPosition = new Vector3((progression - 1) * (18.34f / 2), 0, 0);
    }
}
