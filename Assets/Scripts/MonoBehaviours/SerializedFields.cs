using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SerializedFields : MonoBehaviour
{
    [System.Serializable]
    private class CheckpointNode
    {
        public List<CheckpointNode> before;
        public CheckpointComponentAuthoring checkpoint;
        public List<CheckpointNode> after;
    }
    
    public static SerializedFields singleton;

    public KeyCode keyForward;
    public KeyCode keyBackward;
    public KeyCode keyTurnRight;
    public KeyCode keyTurnLeft;

    public Transform mainCamera;

    public GameObject carMeshPrefab;

    public LocalPlayerHealthUpdater localPlayerHealthUpdater;

    public Cinemachine.CinemachineVirtualCamera virtualCamera;
    public Cinemachine.CinemachineBrain cinemachineBrain;

    public Sprite laserPowerupSprite;
    public Sprite boostPowerupSprite;
    public Sprite shieldPowerupSprite;
    public Sprite repairPowerupSprite;
    public Sprite missilePowerupSprite;

    public Color laserPowerupColor;
    public Color boostPowerupColor;
    public Color shieldPowerupColor;
    public Color repairPowerupColor;
    public Color missilePowerupColor;
    public Color inactivePowerupColor;

    public float maxSteerAngle;
    public float wheelSteeringAngularSpeed;
    public float idleWheelSteeringAngularSpeed;

    public float maxCarSpeed;

    public float forwardAcceleration;
    public float backwardAcceleration;
    public float backwardTopSpeed;
    public float brakeAcceleration;
    public float idleDeacceleration;
    public float angularAcceleration;

    public float friction;

    public float maxAngularVelocity;

    public float maxTurningAngularVelocity;
    public float minSpeedForFullRotation;

    public float wheelLongitudinalDistance;
    public float wheelLateralDistance;

    public float maxHealth;
    public float projectileRelativeSpeed;
    public float projectileDamage;
    public float projectileImpulse;

    public float missileRelativeSpeed;
    public float missileAcceleration;
    public float missileLifetime;
    public float missileDamage;
    public float missileImpulse;
    public float missileRotationSpeed;
    public float missileVirtualTargetMaxAngle;
    public float missileVirtualTargetMaxDistance;
    public float missileVirtualTargetLerpDistance;
    public float numberOfMissilesOnShot;

    public float repairPowerupHealth;
    public float shieldPowerupDuration;
    public float boostPowerupDuration;

    public float boostPowerupAcceleration;

    public float powerupRespawnTime;

    public uint numberOfPowerupSlots;

    public uint laserPowerupShots;

    public float missileMaxTargetAngle;
    public float missileMaxTargetDistance;

    public int deathPenaltyInSeconds;
    public float resurrectionShieldDuration;

    public uint startCountdownSeconds;

    [SerializeField] private List<CheckpointNode> checkpointNodes;
    [SerializeField] private CheckpointComponentAuthoring finishLine;

    public List<Transform> spawnLocations;

    private void Awake()     
    {
        singleton = this;

        List<CheckpointComponentAuthoring> checkpoints = new List<CheckpointComponentAuthoring>();
        uint finishLineCheckpointNumber = 0;

        foreach (var checkpointNode in checkpointNodes)
        {
            AddCheckpoints(checkpointNode, checkpoints, ref finishLineCheckpointNumber);
        }

        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].SetCheckpointNumber(Convert.ToUInt32((i + (checkpoints.Count - finishLineCheckpointNumber)) % checkpoints.Count));
        }

        foreach (var world in World.All)
        {
            var checkpointInitializationSystem = world.GetExistingSystem<CheckpointInitializationSystem>();
            if (checkpointInitializationSystem != null)
            {
                checkpointInitializationSystem.numberOfCheckpoints = Convert.ToUInt32(checkpoints.Count);
            }
        }
    }

    private void AddCheckpoints(CheckpointNode checkpointNode, List<CheckpointComponentAuthoring> checkpoints, ref uint finishLineCheckpointNumber)
    {
        foreach (var beforeCheckpointNode in checkpointNode.before)
        {
            AddCheckpoints(beforeCheckpointNode, checkpoints, ref finishLineCheckpointNumber);
        }

        if (checkpointNode.checkpoint != null)
        {
            if (checkpointNode.checkpoint == finishLine)
            {
                finishLineCheckpointNumber = Convert.ToUInt32(checkpoints.Count);
            }
            
            checkpoints.Add(checkpointNode.checkpoint);
        }
        
        foreach (var afterCheckpointNode in checkpointNode.after)
        {
            AddCheckpoints(afterCheckpointNode, checkpoints, ref finishLineCheckpointNumber);
        }
    }
}
