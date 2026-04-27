using UnityEngine;
using UnityEngine.InputSystem;

public class TrackPoint : MonoBehaviour
{ 
    [SerializeField] private int pointIndex; // the first trackpoint players will hit is 0, the next is 1, and so on. 
    [SerializeField] private int afterCheckPoint; // set this to the checkpoint BEFORE this trackpoint. if that checkpoint is the finish line, set this to 0. 

    private void OnTriggerEnter(Collider other)
    {
        PlacementTracker placement = other.gameObject.GetComponent<PlacementTracker>();
        if (placement != null)
        {
            // handle track point hit
            placement.HandleTrackPointHit(pointIndex);
        }
    }

    public int GetPointIndex()
    {
        return pointIndex;
    }

    public int GetAfterCheckPoint()
    {
        return afterCheckPoint;
    }
}
