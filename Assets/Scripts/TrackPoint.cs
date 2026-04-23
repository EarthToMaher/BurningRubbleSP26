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
            Debug.Log("I detected player " + (placement.gameObject.GetComponent<PlayerInput>().playerIndex + 1));

            // determine which player hit the TrackPoint
            int player = placement.gameObject.GetComponent<PlayerInput>().playerIndex + 1;

            // handle track point hit
            placement.HandleTrackPointHit(pointIndex);
        }
    }

    public int GetPointIndex()
    {
        return pointIndex;
    }
}
