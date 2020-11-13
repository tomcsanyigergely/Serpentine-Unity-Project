using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaveButtonScript : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject allGameObjects;
    
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            List<World> worldsToDispose = new List<World>();
            foreach (var world in World.All)
            {
                if (world.Name.Equals("ServerWorld") || world.Name.Equals("ClientWorld"))
                {
                    worldsToDispose.Add(world);
                }
            }

            foreach (var world in worldsToDispose)
            {
                Debug.Log("Disposing world " + world.Name);
                world.EntityManager.CompleteAllJobs();
                world.Dispose();
            }
            
            allGameObjects.SetActive(false);
            
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        });
    }
}
