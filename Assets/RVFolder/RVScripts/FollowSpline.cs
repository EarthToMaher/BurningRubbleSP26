using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class FollowSpline : MonoBehaviour
{
    // Components
    [SerializeField] private SplineContainer _currentFollowingSpline;

    // Variables
    [SerializeField] private float _speed = 0.01f;
    [SerializeField] private float _duration = 3f;
    private float _time;

    private void OnTriggerEnter(Collider other)
    {
        var _getSpline = other.GetComponent<SplineTriggerZone>();
        _currentFollowingSpline = _getSpline.SplineInTrigger();
        AnimateAlongPathEngage();
    }

    public void AnimateAlongPathEngage()
    {
        StartCoroutine(AnimateAlongPath());
    }

    IEnumerator AnimateAlongPath()
    {
        _time = 0f; // Reset timer

        while (_time < 1f)
        {
            _time += Time.deltaTime / _duration;

            // Evaluates the Splines position relative to the time, then moves the player along that spline (time)
            float3 position = _currentFollowingSpline.EvaluatePosition(_time);
            transform.position = position;

            // Properly aligns the player along the splines curve
            float3 tangent = _currentFollowingSpline.EvaluateTangent(_time);
            transform.rotation = Quaternion.LookRotation(tangent);

            yield return null;
        }
    }
}
