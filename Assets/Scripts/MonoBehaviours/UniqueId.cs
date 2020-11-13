using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class UniqueId : MonoBehaviour
{
    private static Dictionary<string, UniqueId> allIds = new Dictionary<string, UniqueId>();
    
    public String id;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        while (id == null || IdAlreadyUsed())
        {
            id = Guid.NewGuid().ToString();
        }

        if (!allIds.ContainsKey(id))
        {
            allIds.Add(id, this);
        }
    }

    private void OnDestroy()
    {
        allIds.Remove(id);
    }

    private bool IdAlreadyUsed()
    {
        return (id != null && (allIds.ContainsKey(id) && allIds[id] != this));
    }
#endif
}
