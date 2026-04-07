using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EndScreenManager : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> nameText = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> timeText = new List<TextMeshProUGUI>();
    [SerializeField] private List<GameObject> playerPanels = new List<GameObject>();
    [SerializeField] private List<GameObject> menuSelection = new List<GameObject>();
    [SerializeField] private List<Button> menuButtons = new List<Button>();
    [SerializeField] private GameObject endScreen;

    private List<float> finishTimes;
    private List<string> playerNames;
    private UI_InputManager ui;
    private int totalPlayers;
    private bool singlePlayer;
    private Button currButton;

    private void Start()
    {
        finishTimes = new List<float>();
        playerNames = new List<string>();
        ui = FindFirstObjectByType<UI_InputManager>();
        totalPlayers = 1; 
        singlePlayer = true;
        currButton = menuButtons[0];
    }

    private void Update()
    {
        float scrollX = ui.GetJoystickScroll('x');

        if (scrollX != 0 && ui.GetDelayComplete())
        {
            if (scrollX > 0)
            {
                menuSelection[1].SetActive(true);
                menuSelection[0].SetActive(false);
                currButton = menuButtons[1];
            }
            else
            {
                menuSelection[0].SetActive(true);
                menuSelection[1].SetActive(false);
                currButton = menuButtons[0];
            }

            StartCoroutine(ui.JoystickCooldown());
        }

        if (ui.GetConfirmPressed())
        {
            ExecuteEvents.Execute(currButton.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }

    public void PlayerFinish(float time, string name)
    {
        finishTimes.Add(time);
        playerNames.Add(name);

        if(finishTimes.Count == totalPlayers)
        {
            EndRace();
        }
    }

    public void EndRace()
    {
        for(int i=0; i<finishTimes.Count; i++)
        {
            nameText[i].SetText(playerNames[i]);
            timeText[i].SetText(finishTimes[i].ToString());
            playerPanels[i].SetActive(true);
        }

        endScreen.SetActive(true);
    }

    public void SetTotalPlayers(int players)
    {
        totalPlayers = players;
        if(totalPlayers > 1)
        {
            singlePlayer = false;
        }
    }

    public bool GetSinglePlayerMode()
    {
        return singlePlayer;
    }

    public void ClearTimes()
    {
        finishTimes.Clear();
        playerNames.Clear();
        foreach(GameObject panel in playerPanels)
        {
            panel.SetActive(false);
        }
    }
}
