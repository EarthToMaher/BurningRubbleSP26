
using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Car Properties")]
    public float motorTorque = 2000f;
    public float brakeTorque = 2000f;
    public float maxSpeed = 20f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = -1.5f;
    public float maxTurnSpeed;

    private WheelControl[] wheels;
    private Rigidbody rb;

    private CarInputActions carControls;

    void Awake()
    {
        carControls = new CarInputActions();
    }

    void OnEnable()
    {
        carControls.Enable();
    }

    void OnDisable()
    {
        carControls.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Vector3 centerOfMass = rb.centerOfMass;
        centerOfMass.y += centreOfGravityOffset;
        rb.centerOfMass = centerOfMass;

        wheels = GetComponentsInChildren<WheelControl>();
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

        /*float kartRotation = Mathf.Clamp(rb.angularVelocity.y,-maxTurnSpeed,maxTurnSpeed);
        rb.angularVelocity = new Vector3(rb.angularVelocity.x,kartRotation,rb.angularVelocity.z);*/
        rb.maxAngularVelocity = maxTurnSpeed;

        foreach (var wheel in wheels)
        {
            //Apply steering to wheels
            if (wheel.steerable)
            {
                wheel.wheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if(isAccelerating)
            {
                //Apply torque to wheels
                if (wheel.motorized)
                {
                    wheel.wheelCollider.motorTorque = vInput * currentMotorTorque;
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
        }


    }
}
