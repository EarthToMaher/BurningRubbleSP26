using UnityEngine;
using UnityEngine.Splines;

public class FollowSpline : MonoBehaviour
{
    private SplineContainer _currentFollowingSpline;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Spline inside Current Following: " + _currentFollowingSpline);
    }

    private void OnTriggerEnter(Collider other)
    {
        var _getSpline = other.GetComponent<SplineTriggerZone>();
        _currentFollowingSpline = _getSpline.SplineInTrigger();
    }
}
