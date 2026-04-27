using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class TrackPointManager : MonoBehaviour
{
    private List<TrackPoint> trackPoints;
    private List<PlacementTracker> racePlacements;
    private List<int> numTrackPointsPerCP;
    [SerializeField] private int numCheckpoints; // number of checkpoints in the map, including the finish line

    void Start()
    {
        trackPoints = new List<TrackPoint>(
            FindObjectsByType<TrackPoint>(FindObjectsSortMode.None)
        );
        trackPoints.Sort((a, b) => a.GetPointIndex().CompareTo(b.GetPointIndex()));

        racePlacements = new List<PlacementTracker>(
            FindObjectsByType<PlacementTracker>(FindObjectsSortMode.None)
        );

        numTrackPointsPerCP = new List<int>();
        for(int i=0; i<numCheckpoints; i++)
        {
            int numTrackPoints = 0; 

            foreach(TrackPoint tp in trackPoints)
            {
                if(tp.GetAfterCheckPoint() == i)
                {
                    numTrackPoints++;
                }
            }

            numTrackPointsPerCP.Add(numTrackPoints);
        }
    }

    void Update()
    {
        racePlacements.Sort((a, b) => {
            int diff = b.GetNumTrackPointsHit().CompareTo(a.GetNumTrackPointsHit());

            // players hit the same number of TrackPoints. Use distance to the next TrackPoint instead
            if (diff == 0)
            {
                Vector3 playerAPos = a.gameObject.transform.position;
                Vector3 playerBPos = b.gameObject.transform.position;
                Vector3 nextTrackPointPos= GetTrackPointByIndex(a.GetNextTrackPoint()).gameObject.transform.position;

                float distanceA = Vector3.Distance(playerAPos, nextTrackPointPos);
                float distanceB = Vector3.Distance(playerBPos, nextTrackPointPos);
                
                Debug.Log(distanceA + " | " + distanceB);
                diff = distanceA.CompareTo(distanceB);

                // FAIL SAFE: PLAYERS ARE THE SAME DISTANCE FROM THE TRACK POINT
                if(diff == 0)
                {
                    int playerANum = a.gameObject.GetComponent<PlayerInput>().playerIndex;
                    int playerBNum = b.gameObject.GetComponent<PlayerInput>().playerIndex;

                    playerANum.CompareTo(playerBNum);
                }
            }
            return diff;
        });

        for(int p=0; p<racePlacements.Count; p++)
        {
            // racePlacements is sorted by placement logic, so the index+1 is that player's position in the race
            racePlacements[p].SetPlace(p+1);
        }
    }

    private TrackPoint GetTrackPointByIndex(int pointIndex)
    {
        if (pointIndex > 0 && pointIndex < trackPoints.Count)
        {
            return trackPoints[pointIndex];
        }

        Debug.Log("ERROR: TrackPoint index out of bounds exception\nAt TrackPointManager.GetTrackPointByIndex(int)\nReturned TrackPoint at index 0");
        return trackPoints[0];
    }

    public int GetTotalTrackPoints()
    {
        return trackPoints.Count;
    }

    public int GetNumTrackPointsByCheckpoint(int checkpoint) // if the checkpoint to check is the finish line: checkpoint == 0
    {
        if(checkpoint < numCheckpoints && checkpoint >= 0)
        {
            return numTrackPointsPerCP[checkpoint];
        }
        else
        {
            Debug.Log("ERROR: GetNumTrackPointsByCheckpoint | checkpoint parameter out of bounds | returned 0");
            return 0;
        }
    }
}
