using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MPManager : MonoBehaviour
{
    //join screen text
    [SerializeField] private JoinScreenManager joinScreen;

    //array of player prefabs
    [SerializeField] private GameObject[] players;
    [SerializeField] private EndScreenManager endScreen;

    //start positions
    [SerializeField] private Vector3 player1Start;
    [SerializeField] private Vector3 player2Start;
    [SerializeField] private Vector3 player3Start;
    [SerializeField] private Vector3 player4Start;

    //number of players who have joined
    private int numPlayers = 0;

    //called by PlayerInputManager component when join action is triggered
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        //track number of players joined
        numPlayers++;

        //set start positions
        switch(numPlayers)
        {
            case 1:
                playerInput.gameObject.transform.position = player1Start;
                break;
            case 2:
                playerInput.gameObject.transform.position = player2Start;
                break;
            case 3:
                playerInput.gameObject.transform.position = player3Start;
                break;
            case 4:
                playerInput.gameObject.transform.position = player4Start;
                break;
        }

        //set up player array
        players[playerInput.playerIndex] = playerInput.gameObject;

        //set text on the join screen
        joinScreen.SetPlayerJoinedText(numPlayers);
    }

    public void StartGame()
    {
        //removes the join screen UI
        joinScreen.ClearJoinScreen();

        //end screen set up
        endScreen.SetTotalPlayers(numPlayers);

        //loop looks at every player
        foreach(GameObject player in players)
        {
            Debug.Log("Saw a player");
            if(player != null)
            {
                Debug.Log("Player is not null");
                GameObject playerPrefab = player.transform.parent.gameObject;
                //activate cameras
                Camera[] cameras = playerPrefab.GetComponentsInChildren<Camera>(true);
                foreach (Camera cam in cameras)
                {
                    cam.gameObject.SetActive(true);
                    Debug.Log(cam.gameObject.name);
                }

                //trigger countdown & race start
                playerPrefab.GetComponentInChildren<Countdown>().SetGameStarted(true);
            }
        }
    }
}
