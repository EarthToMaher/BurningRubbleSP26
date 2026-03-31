using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerSelection : MonoBehaviour
{
    [SerializeField] private GameObject[] playerIndicators;
    [SerializeField] private GameObject[] joinScreens;
    [SerializeField] float delay;

    private MPManager mp;
    private InputAction playerSelectAction;
    private InputAction confirmAction;
    private bool delayComplete;
    private int numPlayers = 1;
    private GameObject currIndicator;
    private GameObject joinScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mp = FindFirstObjectByType<MPManager>();
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
                currIndicator = playerIndicators[numPlayers - 1];
                currIndicator.SetActive(true);
                currIndicator.transform.GetChild(0).gameObject.SetActive(true); // get the text object that shows the numPlayers selected and set it to active
                playerIndicators[numPlayers - 2].transform.GetChild(0).gameObject.SetActive(false); // set the previously active text object to inactive
            }
            else
            {
                currIndicator = playerIndicators[numPlayers];
                currIndicator.SetActive(false);
                playerIndicators[numPlayers - 1].transform.GetChild(0).gameObject.SetActive(true); // get the text object that shows the numPlayers selected and set it to active
            }

            StartCoroutine(JoystickCooldown());
        }

        if(confirmAction.WasPressedThisFrame())
        {
            GoToJoinScreen();
        }

        //Debug.Log("NumPlayers: " + numPlayers);
    }

    private IEnumerator JoystickCooldown()
    {
        delayComplete = false;
        yield return new WaitForSeconds(delay);
        delayComplete = true;
    }

    private void GoToJoinScreen()
    {
        switch(numPlayers)
        {
            case 1:
                joinScreen = Instantiate(joinScreens[0]);
                break;
            case 2:
                joinScreen = Instantiate(joinScreens[1]);
                break;
            case 3:
                joinScreen = Instantiate(joinScreens[2]);
                break;
            case 4:
                joinScreen = Instantiate(joinScreens[3]);
                break;     
        }

        joinScreen.GetComponent<JoinScreenManager>().SetNumPlayers(numPlayers);
        mp.SetJoinScreen();

        Destroy(gameObject.transform.parent.gameObject);
    }
}
