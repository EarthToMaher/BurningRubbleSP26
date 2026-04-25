using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlacementTracker : MonoBehaviour
{
    private int numTrackPointsHit;
    private int nextPointIndex;
    private int place;
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
        place = placement;

        // handle ui text
        string ending;
        if(place == 1)
        {
            ending = "ST";
        }
        else if(place == 2)
        {
            ending = "ND";
        }
        else if(place == 3)
        {
            ending = "RD";
        }
        else
        {
            ending = "TH";
        }

        placementText.SetText(place + ending);
    }
}
