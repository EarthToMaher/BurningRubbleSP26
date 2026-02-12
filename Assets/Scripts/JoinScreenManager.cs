using UnityEngine;
using UnityEngine.UI;
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


    public void SetPlayerJoinedText(int player)
    {
        switch(player)
        {
            case 1:
                p1Text.SetText("P1 READY");
                startText.gameObject.SetActive(true);
                p1NotJoined.gameObject.SetActive(false);
                p1Joined.gameObject.SetActive(true);
                break;
            case 2:
                p2Text.SetText("P2 READY");
                p2NotJoined.gameObject.SetActive(false);
                p2Joined.gameObject.SetActive(true);
                break;
            case 3:
                p3Text.SetText("P3 READY");
                p3NotJoined.gameObject.SetActive(false);
                p3Joined.gameObject.SetActive(true);
                break;
            case 4:
                p4Text.SetText("P4 READY");
                p4NotJoined.gameObject.SetActive(false);
                p4Joined.gameObject.SetActive(true);
                break;
        }
    }

    public void ClearJoinScreen()
    {
        Destroy(gameObject);
    }
}
