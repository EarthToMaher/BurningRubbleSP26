using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class FollowSpline : MonoBehaviour
{
    // Components
    [SerializeField] private CarControl _kartBoost;
    [SerializeField] private SplineContainer _currentFollowingSpline;
    [SerializeField] private InputActionReference _cancelAction;

    // Variables
    [SerializeField] private float _speed = 0.01f;
    [SerializeField] private float _rubbleBoostDuration = 1f;
    private float _duration = 3f;
    private float _time;

    private void Awake()
    {
        _kartBoost = this.GetComponent<CarControl>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var _getSpline = other.GetComponent<SplineTriggerZone>();
        _currentFollowingSpline = _getSpline.SplineInTrigger();
        _duration = _getSpline.TriggerZoneDuration();
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

            // If accelerate is hit after a couple seconds into the duration, players can immediately hop off of it
            if (_time >= .5f && _cancelAction.action.triggered)
            {
                _kartBoost.RubbleBoost(_rubbleBoostDuration);
                yield break;
            }

            yield return null;
        }
    }
}
