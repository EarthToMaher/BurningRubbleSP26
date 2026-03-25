using UnityEngine;
using UnityEngine.Splines;

public class SplineTriggerZone : MonoBehaviour
{
    [SerializeField] private SplineContainer _splineToFollow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _splineToFollow = GetComponentInParent<SplineContainer>();   
    }

    public SplineContainer SplineInTrigger()
    {
        return _splineToFollow;
    }
}
