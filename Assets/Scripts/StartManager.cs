using UnityEngine;
using UnityEngine.InputSystem;

public class StartManager : MonoBehaviour
{
    public MPManager multiplayer;
    public PlayerInput playerInput;
    private bool gameStarted;
    private JoinScreenManager joinScreen;
    private EndScreenManager endScreenMgr;

    void Awake()
    {
        gameStarted = false;
        playerInput = GetComponent<PlayerInput>();
        multiplayer = FindFirstObjectByType<MPManager>();
        joinScreen = FindFirstObjectByType<JoinScreenManager>();
        endScreenMgr = FindFirstObjectByType<EndScreenManager>(FindObjectsInactive.Include);
    }

    public void OnJoin()
    {
        //starts game if the join action was triggered by player 1 (this will only trigger once)
        if (joinScreen.GetIsFull() && !gameStarted && playerInput.playerIndex == 0)
        {
            multiplayer.StartGame();
            GetComponent<CarControl>().enabled = true;
            gameStarted = true;
            endScreenMgr.gameObject.SetActive(true);
        }
    }
}
