using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private KeyCode usePowerupSlotOne;
    [SerializeField] private KeyCode usePowerupSlotTwo;
    [SerializeField] private KeyCode usePowerupSlotThree;
    
    [SerializeField] private KeyCode dropPowerupSlotOne;
    [SerializeField] private KeyCode dropPowerupSlotTwo;
    [SerializeField] private KeyCode dropPowerupSlotThree;
    
    private InputActionHandlerClientSystem inputActionHandlerClientSystem;

    private void Awake()
    {
        foreach (World world in World.All)
        {
            var inputActionHandlerClientSystem = world.GetExistingSystem<InputActionHandlerClientSystem>();
            if (inputActionHandlerClientSystem != null)
            {
                this.inputActionHandlerClientSystem = inputActionHandlerClientSystem;
                return;
            }
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(usePowerupSlotOne))
        {
            inputActionHandlerClientSystem.QueueUseSlotAction(0);
        }
        if (Input.GetKeyDown(usePowerupSlotTwo))
        {  
            inputActionHandlerClientSystem.QueueUseSlotAction(1);
        }
        if (Input.GetKeyDown(usePowerupSlotThree))
        {
            inputActionHandlerClientSystem.QueueUseSlotAction(2);
        }
        if (Input.GetKeyDown(dropPowerupSlotOne))
        {
            inputActionHandlerClientSystem.QueueDropSlotAction(0);
        }
        if (Input.GetKeyDown(dropPowerupSlotTwo))
        {
            inputActionHandlerClientSystem.QueueDropSlotAction(1);
        }
        if (Input.GetKeyDown(dropPowerupSlotThree))
        {
            inputActionHandlerClientSystem.QueueDropSlotAction(2);
        }
    }
}
