using UnityEngine;
using UnityEngine.UIElements;

public class WheelControl : MonoBehaviour
{
    [SerializeField] private Transform wheelModel;
    [HideInInspector]public WheelCollider wheelCollider;
    public bool steerable;
    [SerializeField] public bool motorized;

    private Vector3 position;
    private Quaternion rotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (wheelModel==null) return;
        //Gets the wheel's position and rotation to set the model's position and rotation
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;
    }
}
