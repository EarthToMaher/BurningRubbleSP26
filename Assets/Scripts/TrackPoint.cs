using UnityEngine;
using UnityEngine.InputSystem;

public class TrackPoint : MonoBehaviour
{
    [SerializeField] private int pointIndex;

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
}
