using UnityEngine;
using UnityEngine.InputSystem;

public class TrackPoint : MonoBehaviour
{
    [SerializeField] private bool trackingPointHit;
    [SerializeField] private int pointIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trackingPointHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlacementTracker placement = other.gameObject.GetComponent<PlacementTracker>();
        if (placement != null)
        {
            Debug.Log("I detected player " + (placement.gameObject.GetComponent<PlayerInput>().playerIndex + 1));
            trackingPointHit = true;
        }
    }

    public void Reset()
    {
        trackingPointHit = false;
    }
}
