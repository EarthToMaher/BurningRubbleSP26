using UnityEngine;
using UnityEngine.InputSystem;

/* 
 * This script must be attached to the kart prefab, on the same object as kart movement.
 * For multiplayer: on the same object as this script and kart movement, should be a PlayerInput component.
 * The PlayerInput component is used by the MultiplayerManager in order to spawn in kart prefabs for each player. 
 * This script links to PlayerInput through the playerInput variable, assigned through a GetComponent.
 * All input actions are assigned through the playerInput variable, which will automatically determine which kart to move based on which controller is providing input.
 */
public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction reverseAction;
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction driftAction;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        reverseAction = playerInput.actions["Reverse"];
        accelerateAction = playerInput.actions["Accelerate"];
        brakeAction = playerInput.actions["Brake"];
        driftAction = playerInput.actions["Drift"]; 
    }

    /* 
    * use this to get the movement direction. 
    * parameter is TRUE if they are reversing, which will flip the movement direction to modify for driving backwards. 
    * parameter is FALSE otherwise, and returns the movement direction unmodified.
    * This returns left/right movement. Left = -1 | Right = 1. (Flipped if isReversing is true)
    */
    public float GetMoveDirectionX(bool isReversing) 
    {
        Vector2 moveDirection;
        if(!isReversing)
        {
            moveDirection = moveAction.ReadValue<Vector2>().normalized;
        }
        else
        {
            moveDirection = -moveAction.ReadValue<Vector2>().normalized;
        }
        return moveDirection.x;
    }

    /*
     * use this to get the value of the reverse input. 
     * it is 1 while the player presses the reverse input, and 0 otherwise. 
     * it needs to be multiplied by a speed value to increase the speed of the kart as it reverses.
     */
    public float GetReverse() 
    {
        return reverseAction.ReadValue<float>();
    }

    /*
     * use this to get the value of the acceleration input. 
     * it is 1 while the player presses the accelerate input, and 0 otherwise. 
     * it needs to be multiplied by a speed value to increase the speed of the kart.
     */
    public float GetAcceleration() 
    {
        return accelerateAction.ReadValue<float>();
    }

    /*
     * use this to get the value of the brake input. 
     * it is 1 while the player presses the brake input, and 0 otherwise.
     */
    public float GetBrake() 
    {
        return brakeAction.ReadValue<float>();
    }

    /*
     * use this to set the value that handles the START of a drift. 
     * it only happens once, during the frame the player starts drifting.
     * Think GetKeyDown() method
     */
    public bool GetStartedDrifting()
    {
        return driftAction.WasPressedThisFrame();
    }

    /*
     * use this to determine that the player is STILL drifting. 
     * When they stop, this turns false, and logic can be updated to end the drift
     * Think GetKey() method
     */
    public bool GetDrifting()
    {
        return driftAction.IsPressed();
    }
}