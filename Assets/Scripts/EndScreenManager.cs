using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EndScreenManager : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> nameText = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> timeText = new List<TextMeshProUGUI>();
    [SerializeField] private List<GameObject> playerPanels = new List<GameObject>();
    [SerializeField] private GameObject endScreen;

    private List<float> finishTimes = new List<float>();
    private List<string> playerNames = new List<string>();
    private int totalPlayers = 1;

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
