using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerFollower : MonoBehaviour
{
    public UnityAction<float> OnUpdateHealth;
    
    [SerializeField] private float wheelRadius;

    [SerializeField] private ProgressBar healthBar;

    [SerializeField] private GameObject crosshair;

    [SerializeField] private GameObject frontLeftWheel;
    [SerializeField] private GameObject frontRightWheel;
    [SerializeField] private GameObject bottomLeftWheel;
    [SerializeField] private GameObject bottomRightWheel;

    [SerializeField] private float healthBarScaleMinDistance;
    [SerializeField] private float crosshairScaleMinDistance;

    [SerializeField] private GameObject shield;

    private PlayerFollowerSystem playerFollowerSystem;
    private MissileTargetClientSystem missileTargetClientSystem;
    private PowerupSlotClientSystem powerupSlotClientSystem;

    public int playerId;

    private float wheelAngleX = 0;

    private void Awake()
    {
        OnUpdateHealth += OnUpdateHealthBar;
        
        foreach (World world in World.All)
        {
            PlayerFollowerSystem playerFollowerSystem = world.GetExistingSystem<PlayerFollowerSystem>();
            if (playerFollowerSystem != null)
            {
                this.playerFollowerSystem = playerFollowerSystem;
            }

            MissileTargetClientSystem missileTargetClientSystem = world.GetExistingSystem<MissileTargetClientSystem>();
            if (missileTargetClientSystem != null)
            {
                this.missileTargetClientSystem = missileTargetClientSystem;
            }

            PowerupSlotClientSystem powerupSlotClientSystem = world.GetExistingSystem<PowerupSlotClientSystem>();
            if (powerupSlotClientSystem != null)
            {
                this.powerupSlotClientSystem = powerupSlotClientSystem;
            }
        }       
    }

    private void Start()
    {
        if (playerFollowerSystem.GetSingleton<NetworkIdComponent>().Value == playerId)
        {
            healthBar.gameObject.SetActive(false);
        }
        
        OnMissileTargetChanged();
    }

    public void OnMissileTargetChanged()
    {
        if (missileTargetClientSystem.MissileTargetPlayerId.HasValue && missileTargetClientSystem.MissileTargetPlayerId.Value == playerId)
        {
            bool hasMissilePowerups = false;
            for (int i = 0; i < powerupSlotClientSystem.slots.Length; i++)
            {
                if (powerupSlotClientSystem.slots[i] == PowerupSlotContent.Missile)
                {
                    hasMissilePowerups = true;
                    break;
                }
            }

            if (hasMissilePowerups)
            {
                crosshair.SetActive(true);
            }
            else
            {
                crosshair.SetActive(false);
            }
        }
        else
        {
            crosshair.SetActive(false);
        }
    }

    public void OnMissilePowerupAcquired()
    {
        if (missileTargetClientSystem.MissileTargetPlayerId.HasValue && missileTargetClientSystem.MissileTargetPlayerId.Value == playerId)
        {
            crosshair.SetActive(true);
        }
        else
        {
            crosshair.SetActive(false);
        }
    }

    public void OnAllMissilePowerupsUsed()
    {
        crosshair.SetActive(false);
    }

    private void OnUpdateHealthBar(float health)
    {
        healthBar.SetProgression(health / SerializedFields.singleton.maxHealth);
    }

#if !UNITY_SERVER
    private void Update()
    {
        if (playerFollowerSystem != null)
        {
            Tuple<Vector3, Quaternion, float, float, bool> interpolatedState = playerFollowerSystem.InterpolateCarState(playerId, Time.time);

            Vector3 previousPosition = transform.position;

            transform.position = interpolatedState.Item1;
            transform.rotation = interpolatedState.Item2;
            float steerAngle = interpolatedState.Item3;
            float health = interpolatedState.Item4;
            shield.SetActive(interpolatedState.Item5);

            Vector3 deltaPosition = transform.position - previousPosition;
            float deltaDistanceForward = Vector3.Dot(deltaPosition, transform.forward);

            frontLeftWheel.transform.localEulerAngles = new Vector3(-wheelAngleX - deltaDistanceForward / wheelRadius / Mathf.PI * 180f, 180 + steerAngle, 0);
            frontRightWheel.transform.localEulerAngles = new Vector3(wheelAngleX + deltaDistanceForward / wheelRadius / Mathf.PI * 180f, 0 + steerAngle, 0);

            wheelAngleX = (wheelAngleX + deltaDistanceForward / wheelRadius / Mathf.PI * 180f) % 360f;

            /*frontLeftWheel.transform.Rotate(new Vector3(-1, 0, 0), deltaDistanceForward / wheelRadius / Mathf.PI * 180f, Space.Self);
            frontRightWheel.transform.Rotate(new Vector3(+1, 0, 0), deltaDistanceForward / wheelRadius / Mathf.PI * 180f, Space.Self);*/
            bottomLeftWheel.transform.Rotate(new Vector3(-1, 0, 0), deltaDistanceForward / wheelRadius / Mathf.PI * 180f, Space.Self);
            bottomRightWheel.transform.Rotate(new Vector3(+1, 0, 0), deltaDistanceForward / wheelRadius / Mathf.PI * 180f, Space.Self);
            
            OnUpdateHealth?.Invoke(health);
        }
        else
        {
            Debug.Log("PlayerFollowerSystem not found!");
        }

        if (playerId == playerFollowerSystem.GetSingleton<NetworkIdComponent>().Value)
        {
            SerializedFields.singleton.cinemachineBrain.ManualUpdate();
        }
    }

    private void LateUpdate()
    {
        if (playerFollowerSystem.GetSingleton<NetworkIdComponent>().Value != playerId)
        {
            Transform mainCamera = SerializedFields.singleton.mainCamera;

            healthBar.transform.position = transform.position + mainCamera.transform.up * 1.5f * Mathf.Max(1, (0.5f * Vector3.Distance(healthBar.transform.position, mainCamera.transform.position) + 0.5f * healthBarScaleMinDistance) / healthBarScaleMinDistance);
            healthBar.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f) * Mathf.Max(1, Vector3.Distance(healthBar.transform.position, mainCamera.transform.position) / healthBarScaleMinDistance);
            healthBar.transform.rotation = mainCamera.transform.rotation;

            crosshair.transform.localScale = new Vector3(1, 1, 1) * Mathf.Max(1, Vector3.Distance(crosshair.transform.position, mainCamera.transform.position) / crosshairScaleMinDistance);
            crosshair.transform.rotation = mainCamera.transform.rotation;
        }
    }
#endif
}
