using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlacementTracker : MonoBehaviour
{
    [SerializeField] private int numTrackPointsHit;
    [SerializeField] private int nextPointIndex;
    [SerializeField] private int place;
    [SerializeField] TextMeshProUGUI placementText;
 
    void Start()
    {
        numTrackPointsHit = 0;

        // player's starting placement is their number (ie. Player 1 = 1st, Player 2 = 2nd, etc)
        place = GetComponent<PlayerInput>().playerIndex + 1;
    }

    public void HandleTrackPointHit(int point)
    {
        if(nextPointIndex == point)
        {
            numTrackPointsHit++;
            nextPointIndex++;
        }
    }

    public void ResetNextPointIndex()
    {
        nextPointIndex = 0;
    }

    public int GetNextTrackPoint()
    {
        return nextPointIndex;
    }

    public int GetNumTrackPointsHit()
    {
        return numTrackPointsHit;
    }

    public int GetPlace()
    {
        return place;
    }

    public void SetPlace(int placement)
    {
        Debug.Log("SetPlace is running! Int received: " + placement);
        place = placement;

        // handle ui text
        string ending;
        if(place == 1)
        {
            Debug.Log("I am in 1st");
            ending = "ST";
        }
        else if(place == 2)
        {
            Debug.Log("I am in 2nd");
            ending = "ND";
        }
        else if(place == 3)
        {
            Debug.Log("I am in 3rd");
            ending = "RD";
        }
        else
        {
            Debug.Log("I am in 4th");
            ending = "TH";
        }

        Debug.Log("SET TEXT: " + place + ending);
        placementText.SetText(place + ending);
    }
}
