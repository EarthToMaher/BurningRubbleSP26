using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerSelection : MonoBehaviour
{
    private InputAction playerSelectAction;
    private InputAction confirmAction;
    [SerializeField] float delay;
    private bool delayComplete;
    private int numPlayers;
    [SerializeField] private GameObject[] playerIndicators;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerSelectAction = InputSystem.actions.FindAction("PlayerSelect");
        confirmAction = InputSystem.actions.FindAction("Confirm");
        delayComplete = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = playerSelectAction.ReadValue<Vector2>().normalized;
        //Debug.Log("Direction.x: " + direction.x);

        if(direction.x != 0 && delayComplete)
        {
            numPlayers += (int)Mathf.Round(direction.x);
            numPlayers = Mathf.Clamp(numPlayers, 1, 4);
            if (direction.x > 0 && numPlayers > 1)
            {
                playerIndicators[numPlayers - 1].SetActive(true);
            }
            else
            {
                playerIndicators[numPlayers].SetActive(false);
            }
            StartCoroutine(JoystickCooldown());
        }

        Debug.Log("NumPlayers: " + numPlayers);

    }

    IEnumerator JoystickCooldown()
    {
        delayComplete = false;
        yield return new WaitForSeconds(delay);
        delayComplete = true;
    }
}
