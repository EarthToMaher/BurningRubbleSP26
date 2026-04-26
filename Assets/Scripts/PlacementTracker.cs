using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlacementTracker : MonoBehaviour
{
    [SerializeField] private int numTrackPointsHit;
    [SerializeField] private int nextPointIndex;
    private int place;
    [SerializeField] private List<GameObject> placementIndicators;
    private GameObject activeIndicator;
    private TrackPointManager tpm;
    [SerializeField] private int currLap;

    void Start()
    {
        // player's starting placement is their number (ie. Player 1 = 1st, Player 2 = 2nd, etc)
        place = GetComponent<PlayerInput>().playerIndex + 1;
        numTrackPointsHit = 0;
        currLap = 0;
        activeIndicator = placementIndicators[place - 1];
        activeIndicator.SetActive(true);
        tpm = FindFirstObjectByType<TrackPointManager>(FindObjectsInactive.Include);
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
        currLap++;
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

    public void ResetAfterDeath(int currCheckpoint)
    {
        numTrackPointsHit = currLap * tpm.GetTotalTrackPoints();
        if (currCheckpoint == 0)
        {
            nextPointIndex = 0;
        }
        else
        {
            int checkpointsBefore = 0;
            for(int i=0; i<currCheckpoint; i++)
            { 
                numTrackPointsHit += tpm.GetNumTrackPointsByCheckpoint(i);
                checkpointsBefore += tpm.GetNumTrackPointsByCheckpoint(i);
            }

            nextPointIndex = checkpointsBefore;
        }
    }
}
