using UnityEngine;

public class WheelControl : MonoBehaviour
{
    [SerializeField][Tooltip("Place to drag in our wheel model so it visually turns. Does nothing if empty")] private Transform wheelModel;
    [HideInInspector]public WheelCollider wheelCollider;
    [Tooltip("Determines if the wheel will turn. Usually only applied to front 2 wheels")] public bool steerable;
    [Tooltip("Determines if the wheel will rotate. Usually applied to all wheels unless wanting to simulate a broken wheel (not needed as of now for our game  but good to have available)")] public bool motorized;

    private Vector3 position;
    private Quaternion rotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>(); //Get our WheelCollider component
    }

    // Update is called once per frame
    void Update()
    {
        //Do nothing if no wheel model
        if (wheelModel==null) return;
        //Gets the wheel's position and rotation to set the model's position and rotation
        wheelCollider.GetWorldPose(out position, out rotation);
        wheelModel.transform.position = position;
        wheelModel.transform.rotation = rotation;
    }
}
