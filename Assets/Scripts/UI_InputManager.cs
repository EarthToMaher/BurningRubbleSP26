using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UI_InputManager : MonoBehaviour
{
    private InputAction joystickScrollAction;
    private InputAction confirmAction;
    private bool delayComplete;
    [SerializeField] float scrollDelay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        joystickScrollAction = InputSystem.actions.FindAction("JoystickScroll");
        confirmAction = InputSystem.actions.FindAction("Confirm");
        delayComplete = true;
    }

    public float GetJoystickScroll(char axis)
    {
        Vector2 direction = joystickScrollAction.ReadValue<Vector2>().normalized;
        if (axis == 'x')
        {
            return direction.x;
        }
        else if(axis == 'y')
        {
            return direction.y;
        }
        else
        {
            Debug.Log("ERROR: GetJoystickScroll\nInvalid Parameter, returned 0.\nUse x or y as char value for this method.");
            return 0f;
        }
    }

    public bool GetConfirmPressed()
    {
        return confirmAction.WasPressedThisFrame();
    }

    public bool GetDelayComplete()
    {
        return delayComplete;
    }

    public IEnumerator JoystickCooldown()
    {
        delayComplete = false;
        yield return new WaitForSeconds(scrollDelay);
        delayComplete = true;
    }
}
