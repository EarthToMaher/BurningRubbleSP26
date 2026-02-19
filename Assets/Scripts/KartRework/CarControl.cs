

using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Car Properties")]
    [Tooltip("The speed at which the wheels accelerate")]public float motorTorque = 2000f;
    [Tooltip("The speed at which the wheels decelerate")]public float brakeTorque = 2000f;
    [Tooltip("Max speed of the car")]public float maxSpeed = 20f;
    [Tooltip("How far the front wheels turn")]public float steeringRange = 30f;
    [Tooltip("How far the front wheels turn at max speed")] public float steeringRangeAtMaxSpeed = 10f;
    [Tooltip("Used for creating weight. Lower center of gravity = more weight")]public float centreOfGravityOffset = -1.5f;
    [Tooltip("How far the car is actually allowed to turn. Prevents splipping")]public float maxTurnSpeed;

    
    private WheelControl[] wheels; //Array of wheels
    private Rigidbody rb; //Kart RB
    private CarInputActions carControls; //Our current control mapping, will be replaced with our new InputActions

    void Awake()
    {
        carControls = new CarInputActions(); //Get our controls
    }

    void OnEnable()
    {
        carControls.Enable(); //enables them if car is active
    }

    void OnDisable()
    {
        carControls.Disable(); //Disables them if not
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>(); //Get RB

        //Apply center of mass to give kart weight
        Vector3 centerOfMass = rb.centerOfMass;
        centerOfMass.y += centreOfGravityOffset;
        rb.centerOfMass = centerOfMass;

        wheels = GetComponentsInChildren<WheelControl>(); //Get Wheels
    }

    void FixedUpdate()
    {
        
        //Read our input
        Vector2 inputVector = carControls.Car.Movement.ReadValue<Vector2>();

        //Assign each one to a float for acceleration and steering
        float vInput = inputVector.y;
        float hInput = inputVector.x;

        //Caculate speed
        float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0,maxSpeed,Mathf.Abs(forwardSpeed)); //Normalize speed factor

        //Reduce motor torque and steering at high speeds for better handling
        float currentMotorTorque = Mathf.Lerp(motorTorque,0,speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange,steeringRangeAtMaxSpeed, speedFactor);

        //Determine if player is accelerating or reversing
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        //Define our array of normals for alignment and an index for which spot of the array we are in
        Vector3[] normals = new Vector3[4];
        int index = 0;

        //Go through logic applied to each wheel
        foreach (var wheel in wheels)
        {
            //Apply steering to wheels
            if (wheel.steerable)
            {
                wheel.wheelCollider.steerAngle = hInput * currentSteerRange; //Turns wheel

            }

            if(isAccelerating)
            {
                //Apply torque to wheels
                if (wheel.motorized)
                {
                    wheel.wheelCollider.motorTorque = vInput * currentMotorTorque; //Rotates the wheel to accelerate
                }

                //Release brakes when accelerating
                wheel.wheelCollider.brakeTorque = 0f;
            }
            else
            {
                // Apply brakes when reversing direction
                wheel.wheelCollider.motorTorque = 0f;
                wheel.wheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
            }

            //Raycast from each of our wheel to get the normals for alignment
            RaycastHit hit;
            if(Physics.Raycast(wheel.transform.position,-wheel.transform.up, out hit)) //Currently always true, should shorten the array then call each wheel if 1 hits
            {
                normals[index]=hit.normal;//Store our normal
            }
            index++; //Update index
        }

        //Ground alignment logic
        Vector3 groundNormal = Vector3.Normalize(normals[0]+ normals[1] + normals[2] + normals[3]);//Add our normals and normalize them to get our ground normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up,groundNormal) * transform.rotation; //Gets our target rotation by getting the difference between our ground and up normal, then multiplying it by our current rotation
        transform.rotation =  Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3); //Smoothly interps between them

        //Logic for clamping our Kart rotation to avoid unnecessary rotations or spin outs
        float kartRotation = Mathf.Clamp(rb.angularVelocity.y,-maxTurnSpeed,maxTurnSpeed);
        rb.angularVelocity = new Vector3(0f,kartRotation,0f);


    }
}
