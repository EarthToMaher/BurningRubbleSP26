using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    [SerializeField] private GameObject[] playerIndicators;
    [SerializeField] private GameObject[] joinScreens;

    private MPManager mp;
    private UI_InputManager ui;
    private int numPlayers;
    private GameObject currIndicator;
    private GameObject joinScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mp = FindFirstObjectByType<MPManager>();
        ui = FindFirstObjectByType<UI_InputManager>();
        numPlayers = 1;
    }

    // Update is called once per frame
    void Update()
    {
        float scrollX = ui.GetJoystickScroll('x');

        if(scrollX != 0 && ui.GetDelayComplete())
        {
            numPlayers += (int)Mathf.Round(scrollX);
            numPlayers = Mathf.Clamp(numPlayers, 1, 4);
            if (scrollX > 0 && numPlayers > 1)
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
