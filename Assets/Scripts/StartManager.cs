using UnityEngine;
using UnityEngine.InputSystem;

public class StartManager : MonoBehaviour
{
    public MPManager multiplayer;
    public PlayerInput playerInput;
    private bool gameStarted = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        multiplayer = FindFirstObjectByType<MPManager>();
    }

    public void OnJoin()
    {
        //starts game if the join action was triggered by player 1 (this will only trigger once)
        if (!gameStarted && playerInput.playerIndex == 0)
        {
            multiplayer.StartGame();
            gameStarted = true;
        }
    }
}
