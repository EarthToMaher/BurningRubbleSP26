using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class JoinScreenManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI p1Text;
    [SerializeField] private TextMeshProUGUI p2Text;
    [SerializeField] private TextMeshProUGUI p3Text;
    [SerializeField] private TextMeshProUGUI p4Text;
    [SerializeField] private TextMeshProUGUI startText;
    [SerializeField] private RawImage p1NotJoined;
    [SerializeField] private RawImage p2NotJoined;
    [SerializeField] private RawImage p3NotJoined;
    [SerializeField] private RawImage p4NotJoined;
    [SerializeField] private RawImage p1Joined;
    [SerializeField] private RawImage p2Joined;
    [SerializeField] private RawImage p3Joined;
    [SerializeField] private RawImage p4Joined;

    private PlayerInputManager playerInputMgr;
    private int maxPlayers = 0;

    void Awake()
    {
        playerInputMgr = FindFirstObjectByType<PlayerInputManager>();
        Debug.Log("Player input manager: " + playerInputMgr == null);
    }

    void Update()
    {
        // check if all players have joined
        if (playerInputMgr.joiningEnabled && playerInputMgr.playerCount == maxPlayers)
        {
            playerInputMgr.DisableJoining();
            Debug.Log("Game is full");
        }
    }

    public void SetPlayerJoinedText(int player)
    {
        switch(player)
        {
            case 1:
                if(maxPlayers == 1)
                {
                    p1Text.SetText("PLAYER 1: READY");
                }
                else
                {
                    p1Text.SetText("PLAYER 1:\nREADY");
                }
                startText.gameObject.SetActive(true);
                p1NotJoined.gameObject.SetActive(false);
                p1Joined.gameObject.SetActive(true);
                break;
            case 2:
                p2Text.SetText("PLAYER 2:\nREADY");
                p2NotJoined.gameObject.SetActive(false);
                p2Joined.gameObject.SetActive(true);
                break;
            case 3:
                p3Text.SetText("PLAYER 3:\nREADY");
                p3NotJoined.gameObject.SetActive(false);
                p3Joined.gameObject.SetActive(true);
                break;
            case 4:
                p4Text.SetText("PLAYER 4:\nREADY");
                p4NotJoined.gameObject.SetActive(false);
                p4Joined.gameObject.SetActive(true);
                break;
        }
    }

    public void ClearJoinScreen()
    {
        Destroy(gameObject);
    }

    public void SetNumPlayers(int players)
    {
        maxPlayers = players;
        playerInputMgr.EnableJoining();
    }
}
