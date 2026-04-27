using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    [SerializeField] private GameObject[] playerIndicators;
    [SerializeField] private GameObject[] joinScreens;

    private MPManager mp;
    private UI_InputManager ui;
    private int numPlayers;
    private GameObject currIndicator;
    private int currIndex;
    private GameObject joinScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mp = FindFirstObjectByType<MPManager>();
        ui = FindFirstObjectByType<UI_InputManager>();
        numPlayers = 1;
        currIndex = 0;
        currIndicator = playerIndicators[currIndex];
    }

    // Update is called once per frame
    void Update()
    {
        float scrollX = ui.GetJoystickScroll('x');

        if(scrollX != 0 && ui.GetDelayComplete())
        {
            numPlayers += (int)Mathf.Round(scrollX);
            numPlayers = Mathf.Clamp(numPlayers, 1, 4);
            currIndicator.SetActive(false);
            currIndicator = playerIndicators[numPlayers - 1];
            currIndicator.SetActive(true);
            StartCoroutine(ui.JoystickCooldown());
        }

        if(ui.GetConfirmPressed())
        {
            GoToJoinScreen();
        }

        //Debug.Log("NumPlayers: " + numPlayers);
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
