using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

public class Setup : MonoBehaviour
{
    [SerializeField] private GameObject allGameObjects;

    [SerializeField] private NetworkListener networkListenerPrefab;

    /*private bool initialized = false;

    private float timeToInitialize = 2f;*/
    
    private void Start()
    {
#if UNITY_EDITOR
        if (GameSession.clientSession == null)
        {
            GameSession.serverSession = new ServerSession
            {
                hostName = "UnityEditor",
                numberOfPlayers = 1,
                laps = 2,
                serverPort = 7979
            };
            GameSession.clientSession = new ClientSession
            {
                remoteServerIpAddress = "127.0.0.1",
                remoteServerPort = 7979
            };
        }
#endif
        
#if !UNITY_SERVER
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<TestInitSystem>().actions.Enqueue(() =>
        {
            var clientWorld = ClientServerBootstrap.CreateClientWorld(World.DefaultGameObjectInjectionWorld, "ClientWorld");
            if (GameSession.serverSession != null)
            {
                var serverWorld = ClientServerBootstrap.CreateServerWorld(World.DefaultGameObjectInjectionWorld, "ServerWorld");
                GameObject.Instantiate(networkListenerPrefab);
            }

            allGameObjects.SetActive(true);
        });
#else
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<TestInitSystem>().actions.Enqueue(() =>
        {
            var serverWorld = ClientServerBootstrap.CreateServerWorld(World.DefaultGameObjectInjectionWorld, "ServerWorld");
            allGameObjects.SetActive(true);
        });
#endif
    }

    /*private void LateUpdate()
    {
        if (initialized)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
            timeToInitialize -= Time.deltaTime;
            if (timeToInitialize <= 0)
            {
                World.DefaultGameObjectInjectionWorld.GetExistingSystem<TestInitSystem>().actions.Enqueue(() =>
                {
                    var clientWorld = ClientServerBootstrap.CreateClientWorld(World.DefaultGameObjectInjectionWorld, "ClientWorld");
                    var serverWorld = ClientServerBootstrap.CreateServerWorld(World.DefaultGameObjectInjectionWorld, "ServerWorld");
                    allGameObjects.SetActive(true);
                    initialized = true;
                });
            }
        }
    }*/
}
