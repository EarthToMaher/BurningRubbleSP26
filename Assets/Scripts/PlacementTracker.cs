using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlacementTracker : MonoBehaviour
{
    private int numTrackPointsHit;
    private int nextPointIndex;
    private int place;
    [SerializeField] private List<GameObject> placementIndicators;
    private GameObject activeIndicator;

    void Start()
    {
        numTrackPointsHit = 0;

        // player's starting placement is their number (ie. Player 1 = 1st, Player 2 = 2nd, etc)
        place = GetComponent<PlayerInput>().playerIndex + 1;
        activeIndicator = placementIndicators[place - 1];
        activeIndicator.SetActive(true);
    }

    public void HandleTrackPointHit(int point)
    {
        if (nextPointIndex == point)
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
        activeIndicator.SetActive(false);
        activeIndicator = placementIndicators[placement - 1];
        activeIndicator.SetActive(true);
    }
}
