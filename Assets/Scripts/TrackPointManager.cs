using UnityEngine;

public class TrackPointManager : MonoBehaviour
{
    [SerializeField] private TrackPoint[] trackPoints;

    void Start()
    {
        trackPoints = FindObjectsByType<TrackPoint>(FindObjectsSortMode.None);
    }

    public void ResetTrackPoints()
    {
        foreach(TrackPoint point in trackPoints)
        {
            point.Reset();
        }
    }
}
