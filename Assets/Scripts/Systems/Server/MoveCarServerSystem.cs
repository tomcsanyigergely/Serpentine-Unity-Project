using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using System;
using System.Numerics;
using Unity.Rendering;
using Math = Unity.Physics.Math;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[UpdateInGroup(typeof(ServerSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
[UpdateAfter(typeof(GhostSimulationSystemGroup))]
public class MoveCarServerSystem : ComponentSystem
{
    private float simulationDeltaTime;

    private GhostPredictionSystemGroup ghostPredictionSystemGroup;
    private StartCountdownServerSystem startCountdownServerSystem;
    private CheckpointInitializationSystem checkpointInitializationSystem;
    
    protected override void OnCreate()
    {
        simulationDeltaTime = 1f / GetSingleton<ClientServerTickRate>().SimulationTickRate;

        ghostPredictionSystemGroup = World.GetOrCreateSystem<GhostPredictionSystemGroup>();
        startCountdownServerSystem = World.GetOrCreateSystem<StartCountdownServerSystem>();
        checkpointInitializationSystem = World.GetOrCreateSystem<CheckpointInitializationSystem>();
    }

    protected override void OnUpdate()
    {
        if (startCountdownServerSystem.raceHasStarted)
        {
            var tick = ghostPredictionSystemGroup.PredictingTick;

            Entities.ForEach((Entity carEntity, DynamicBuffer<PlayerInput> inputBuffer, ref SynchronizedCarComponent carComponent, ref HealthComponent healthComponent, ref CarComponent serverCarComponent, ref PhysicsVelocity velocity, ref Rotation rotation) =>
            {
                PlayerInput input;
                inputBuffer.GetDataAtTick(tick, out input);

                bool playerCanControl = healthComponent.Health > 0 && EntityManager.GetComponentData<ProgressionComponent>(carEntity).CrossedCheckpoints < checkpointInitializationSystem.numberOfCheckpoints * GameSession.serverSession.laps;

                bool keyForwardPressed = Convert.ToBoolean(input.keyForwardPressed) && playerCanControl;
                bool keyBackwardPressed = Convert.ToBoolean(input.keyBackwardPressed) && playerCanControl;
                bool keyTurnRightPressed = Convert.ToBoolean(input.keyTurnRightPressed) && playerCanControl;
                bool keyTurnLeftPressed = Convert.ToBoolean(input.keyTurnLeftPressed) && playerCanControl;

                if (keyTurnRightPressed && !keyTurnLeftPressed)
                {
                    if (carComponent.SteerAngle < 0)
                    {
                        carComponent.SteerAngle = 0;
                    }
                    if (carComponent.SteerAngle < SerializedFields.singleton.maxSteerAngle)
                    {
                        carComponent.SteerAngle += SerializedFields.singleton.wheelSteeringAngularSpeed * simulationDeltaTime;
                        if (carComponent.SteerAngle > SerializedFields.singleton.maxSteerAngle)
                        {
                            carComponent.SteerAngle = SerializedFields.singleton.maxSteerAngle;
                        }
                    }
                }
                else if (keyTurnLeftPressed && !keyTurnRightPressed)
                {
                    if (carComponent.SteerAngle > 0)
                    {
                        carComponent.SteerAngle = 0;
                    }
                    if (carComponent.SteerAngle > -SerializedFields.singleton.maxSteerAngle)
                    {
                        carComponent.SteerAngle -= SerializedFields.singleton.wheelSteeringAngularSpeed * simulationDeltaTime;
                        if (carComponent.SteerAngle < -SerializedFields.singleton.maxSteerAngle)
                        {
                            carComponent.SteerAngle = -SerializedFields.singleton.maxSteerAngle;
                        }
                    }
                }
                else if (!keyTurnRightPressed && !keyTurnLeftPressed)
                {
                    if (carComponent.SteerAngle > 0)
                    {
                        carComponent.SteerAngle -= SerializedFields.singleton.idleWheelSteeringAngularSpeed * simulationDeltaTime;
                        if (carComponent.SteerAngle < 0)
                        {
                            carComponent.SteerAngle = 0;
                        }
                    }
                    else if (carComponent.SteerAngle < 0)
                    {
                        carComponent.SteerAngle += SerializedFields.singleton.idleWheelSteeringAngularSpeed * simulationDeltaTime;
                        if (carComponent.SteerAngle > 0)
                        {
                            carComponent.SteerAngle = 0;
                        }
                    }
                }

                float turningRadiusInverse = CalculateTurningRadiusInverse(carComponent.SteerAngle);

                Vector3 localVelocity = math.mul(math.inverse(rotation.Value), velocity.Linear);
                if (localVelocity.magnitude > 0)
                {
                    Vector3 resistance = -localVelocity.normalized * localVelocity.sqrMagnitude * SerializedFields.singleton.forwardAcceleration / math.pow(SerializedFields.singleton.maxCarSpeed, 2) * simulationDeltaTime;
                    localVelocity += resistance;
                }

                if (localVelocity.x > 0)
                {
                    localVelocity -= new Vector3(SerializedFields.singleton.friction * 9.81f * simulationDeltaTime, 0, 0);
                    if (localVelocity.x < 0)
                    {
                        localVelocity = new Vector3(0, localVelocity.y, localVelocity.z);
                    }
                }
                else if (localVelocity.x < 0)
                {
                    localVelocity += new Vector3(SerializedFields.singleton.friction * 9.81f * simulationDeltaTime, 0, 0);
                    if (localVelocity.x > 0)
                    {
                        localVelocity = new Vector3(0, localVelocity.y, localVelocity.z);
                    }
                }

                if (!serverCarComponent.GoingBackward)
                {
                    if (keyForwardPressed && !keyBackwardPressed)
                    {
                        if (localVelocity.z < 0)
                        {
                            localVelocity += new Vector3(0, 0, SerializedFields.singleton.friction * 9.81f * simulationDeltaTime);
                        }
                        else
                        {
                            localVelocity += new Vector3(0, 0, SerializedFields.singleton.forwardAcceleration * simulationDeltaTime);
                        }
                    }
                    else if (keyBackwardPressed && !keyForwardPressed)
                    {
                        if (localVelocity.z > 0)
                        {
                            localVelocity -= new Vector3(0, 0, SerializedFields.singleton.brakeAcceleration * simulationDeltaTime);
                            if (localVelocity.z < 0)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                            }
                        }
                        else if (localVelocity.z < 0)
                        {
                            localVelocity += new Vector3(0, 0, SerializedFields.singleton.brakeAcceleration * simulationDeltaTime);
                            if (localVelocity.z > 0)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                            }
                        }

                        if (Vector3.ProjectOnPlane(localVelocity, Vector3.up).magnitude < 0.1f)
                        {
                            serverCarComponent.GoingBackward = true;
                        }
                    }
                    else
                    {
                        if (localVelocity.z > 0)
                        {
                            localVelocity -= new Vector3(0, 0, SerializedFields.singleton.idleDeacceleration * simulationDeltaTime);
                            if (localVelocity.z < 0)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                            }
                        }
                        else if (localVelocity.z < 0)
                        {
                            localVelocity += new Vector3(0, 0, SerializedFields.singleton.friction * 9.81f * simulationDeltaTime);
                            if (localVelocity.z > 0)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                            }
                        }
                    }
                }
                else
                {
                    if (keyForwardPressed && !keyBackwardPressed)
                    {
                        if (localVelocity.z > 0)
                        {
                            localVelocity -= new Vector3(0, 0, SerializedFields.singleton.brakeAcceleration * simulationDeltaTime);
                            if (localVelocity.z < 0)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                            }
                        }
                        else if (localVelocity.z < 0)
                        {
                            localVelocity += new Vector3(0, 0, SerializedFields.singleton.brakeAcceleration * simulationDeltaTime);
                            if (localVelocity.z > 0)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                            }
                        }

                        if (Vector3.ProjectOnPlane(localVelocity, Vector3.up).magnitude < 0.1f)
                        {
                            serverCarComponent.GoingBackward = false;
                        }
                    }
                    else if (keyBackwardPressed && !keyForwardPressed)
                    {
                        if (localVelocity.z > -SerializedFields.singleton.backwardTopSpeed)
                        {
                            localVelocity -= new Vector3(0, 0, SerializedFields.singleton.backwardAcceleration * simulationDeltaTime);
                            if (localVelocity.z < -SerializedFields.singleton.backwardTopSpeed)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, -SerializedFields.singleton.backwardTopSpeed);
                            }
                        }
                        else if (localVelocity.z < -SerializedFields.singleton.backwardTopSpeed)
                        {
                            localVelocity += new Vector3(0, 0, SerializedFields.singleton.brakeAcceleration * simulationDeltaTime);
                            if (localVelocity.z > -SerializedFields.singleton.backwardTopSpeed)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, -SerializedFields.singleton.backwardTopSpeed);
                            }
                        }
                    }
                    else
                    {
                        if (localVelocity.z > 0)
                        {
                            localVelocity -= new Vector3(0, 0, SerializedFields.singleton.friction * 9.81f * simulationDeltaTime);
                            if (localVelocity.z < 0)
                            {
                                localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                            }
                        }
                        else if (localVelocity.z < 0)
                        {
                            if (localVelocity.z < -SerializedFields.singleton.backwardTopSpeed)
                            {
                                localVelocity += new Vector3(0, 0, SerializedFields.singleton.friction * 9.81f * simulationDeltaTime);
                            }
                            else
                            {
                                localVelocity += new Vector3(0, 0, SerializedFields.singleton.idleDeacceleration * simulationDeltaTime);
                                if (localVelocity.z > 0)
                                {
                                    localVelocity = new Vector3(localVelocity.x, localVelocity.y, 0);
                                }
                            }
                        }
                    }
                }

                Vector3 localAngularVelocity = velocity.Angular; // no need to inverse transform

                if (localAngularVelocity.y > SerializedFields.singleton.maxAngularVelocity)
                {
                    localAngularVelocity.y = SerializedFields.singleton.maxAngularVelocity;
                }
                else if (localAngularVelocity.y < -SerializedFields.singleton.maxAngularVelocity)
                {
                    localAngularVelocity.y = -SerializedFields.singleton.maxAngularVelocity;
                }

                Vector3 transformUp = math.mul(rotation.Value, new float3(0, 1, 0));

                float targetAngularVelocityY = Mathf.Min(Vector3.ProjectOnPlane(velocity.Linear, transformUp).magnitude / SerializedFields.singleton.minSpeedForFullRotation, 1) * carComponent.SteerAngle / SerializedFields.singleton.maxSteerAngle * SerializedFields.singleton.maxTurningAngularVelocity * (serverCarComponent.GoingBackward ? -1 : 1);

                if (targetAngularVelocityY > localAngularVelocity.y)
                {
                    localAngularVelocity += new Vector3(0, SerializedFields.singleton.angularAcceleration * simulationDeltaTime, 0);
                    if (localAngularVelocity.y > targetAngularVelocityY)
                    {
                        localAngularVelocity = new Vector3(localAngularVelocity.x, targetAngularVelocityY, localAngularVelocity.z);
                    }
                }
                else if (targetAngularVelocityY < localAngularVelocity.y)
                {
                    localAngularVelocity -= new Vector3(0, SerializedFields.singleton.angularAcceleration * simulationDeltaTime, 0);
                    if (localAngularVelocity.y < targetAngularVelocityY)
                    {
                        localAngularVelocity = new Vector3(localAngularVelocity.x, targetAngularVelocityY, localAngularVelocity.z);
                    }
                }

                localVelocity = Vector3.ProjectOnPlane(localVelocity, Vector3.forward) + Quaternion.AngleAxis(localAngularVelocity.y * simulationDeltaTime, Vector3.up) * Vector3.Project(localVelocity, Vector3.forward);

                if (EntityManager.GetComponentData<BoostComponent>(carEntity).RemainingTime > 0)
                {
                    localVelocity += new Vector3(0, 0, SerializedFields.singleton.boostPowerupAcceleration * simulationDeltaTime);
                }
                
                velocity.Angular = localAngularVelocity; // no need to transform
                velocity.Linear = math.mul(rotation.Value, localVelocity);
            });
        }
    }

    private static float CalculateTurningRadiusInverse(float steerAngle)
    {
        if (steerAngle > 0)
        {
            float c = SerializedFields.singleton.wheelLongitudinalDistance;
            float w = SerializedFields.singleton.wheelLateralDistance;
            float tangent = Mathf.Tan(Mathf.Abs(steerAngle) / 180.0f * Mathf.PI);

            return tangent / Mathf.Sqrt(c * c + w * c * tangent + w * w / 4 * tangent * tangent + c * c / 4 * tangent * tangent);
        }
        else if (steerAngle < 0)
        {
            float c = SerializedFields.singleton.wheelLongitudinalDistance;
            float w = SerializedFields.singleton.wheelLateralDistance;
            float tangent = Mathf.Tan(Mathf.Abs(steerAngle) / 180.0f * Mathf.PI);

            return -tangent / Mathf.Sqrt(c * c + w * c * tangent + w * w / 4 * tangent * tangent + c * c / 4 * tangent * tangent);
        }
        else
        {
            return 0;
        }
    }
}