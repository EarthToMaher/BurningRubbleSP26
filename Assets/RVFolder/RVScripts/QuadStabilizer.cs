using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class QuadStabilizer : MonoBehaviour
{
    [SerializeField] private Transform _kart;   // Object to follow (e.g., player)
    [SerializeField] private float _height = 50f;  // Height above the target

    private Vector3 rotationOffset;

    
    void Start()
    {
        // Store initial X and Z rotation relative to world
        rotationOffset = new Vector3(transform.eulerAngles.x, 0f, transform.eulerAngles.z);
        // Kart MUST be the third child in order for this to work.
        _kart = transform.parent.GetChild(2);
    }

    void Update()
    {
        if (_kart == null) return;

        // 1. Position: directly above the target
        transform.position = _kart.position + Vector3.up * _height;

        // 2. Rotation: match target's Y rotation, keep X and Z fixed
        float targetY = _kart.eulerAngles.y;
        transform.rotation = Quaternion.Euler(rotationOffset.x, targetY, rotationOffset.z);
    }
}