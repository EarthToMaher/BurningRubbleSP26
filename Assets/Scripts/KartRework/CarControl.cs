

using System.Collections;
using UltimateProceduralPrimitivesFREE;
using UnityEditor;
using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Car Properties")]
    [Tooltip("The speed at which the wheels accelerate")] public float motorTorque = 2000f;
    [Tooltip("The speed at which the wheels decelerate")] public float brakeTorque = 2000f;
    [Tooltip("Max speed of the car")] public float maxSpeed = 20f;
    [Tooltip("How far the front wheels turn")] public float steeringRange = 30f;
    [Tooltip("How far the front wheels turn at max speed")] public float steeringRangeAtMaxSpeed = 10f;
    [Tooltip("Used for creating weight. Lower center of gravity = more weight")] public float centreOfGravityOffset = -1.5f;
    [Tooltip("How far the car is actually allowed to turn. Prevents splipping")] public float maxTurnSpeed;

    [Header("Drift Controls")]
    [Tooltip("How far the car is actually allowed to turn when drifting. Prevents spinning out")] public float maxDriftSpeed;
    public float jumpStrength;
    public float rotationTime = 3f;
    public float initialRoatateAmount;
    public float driftSpeedThreshold;
    public bool drifting = false;
    private float driftInitialYaw;
    public int driftDirection;
    [Tooltip("How far the front wheels turn")] public float driftSteeringRange = 30f;
    [Tooltip("How far the front wheels turn at max speed")] public float driftSteeringRangeAtMaxSpeed = 10f;
    private float boostVal = 0;



    private WheelControl[] wheels; //Array of wheels
    private Rigidbody rb; //Kart RB
    private CarInputActions carControls; //Our current control mapping, will be replaced with our new InputActions
    private InputManager im;
    public bool grounded = false;

    void Awake()
    {
        carControls = new CarInputActions(); //Get our controls
        im = GetComponent<InputManager>();
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

    public void Update()
    {
        float hInput = im.GetMoveDirectionX();
        float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed)); //Normalize speed factor
        GroundedCheck();
        if (im.GetStartedDrifting())
        {
            StartDrift(hInput, speedFactor);
        }
    }

    void FixedUpdate()
    {
        if (im.GetReload() > 0) FindFirstObjectByType<SceneReload>().Reload();

        //Assign each one to a float for acceleration and steering
        float vInput = im.GetMoveDirectionY();
        float hInput = im.GetMoveDirectionX();
        float acceleration = im.GetAcceleration() - im.GetReverse();
        Debug.LogWarning(acceleration);

        //Caculate speed
        float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed)); //Normalize speed factor

        //Reduce motor torque and steering at high speeds for better handling
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        //Determine if player is accelerating or reversing
        bool isAccelerating = Mathf.Sign(acceleration) == Mathf.Sign(forwardSpeed);

        if (drifting)
        {
            currentSteerRange = Mathf.Lerp(driftSteeringRange, driftSteeringRangeAtMaxSpeed, speedFactor);
            WheelMovement(hInput, acceleration, currentSteerRange, currentMotorTorque, isAccelerating);
            Drift();
                        //Logic for clamping our Kart rotation to avoid unnecessary rotations or spin outs
            float kartRotation = Mathf.Clamp(rb.angularVelocity.y, -maxDriftSpeed, maxDriftSpeed);
            rb.angularVelocity = new Vector3(0f,kartRotation,0f);
        }
        else
        {
            WheelMovement(hInput, acceleration, currentSteerRange, currentMotorTorque, isAccelerating);
            //Logic for clamping our Kart rotation to avoid unnecessary rotations or spin outs
            float kartRotation = Mathf.Clamp(rb.angularVelocity.y, -maxTurnSpeed, maxTurnSpeed);

            rb.angularVelocity = new Vector3(0f,kartRotation,0f);
        }




    }

    public void Boost()
    {

    }

    public void GroundedCheck()
    {
        grounded = false;
        foreach (var wheel in wheels)
        {
            RaycastHit groundHit;
            if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out groundHit, wheel.GetComponent<WheelCollider>().radius*2.5f))
            {
                grounded = true;
                return;
            } 
        }
    }

    public void WheelMovement(float hInput, float vInput, float currentSteerRange, float currentMotorTorque, bool isAccelerating)
    {
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

            if (isAccelerating)
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
            if (Physics.Raycast(wheel.transform.position, -wheel.transform.up, out hit)) //Currently always true, should shorten the array then call each wheel if 1 hits
            {
                normals[index] = hit.normal;//Store our normal
            }
            index++; //Update index
        }

        if (!grounded) return;
        //Ground alignment logic
        Debug.LogWarning("Aligning");
        Vector3 groundNormal = Vector3.Normalize(normals[0] + normals[1] + normals[2] + normals[3]);//Add our normals and normalize them to get our ground normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation; //Gets our target rotation by getting the difference between our ground and up normal, then multiplying it by our current rotation
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 10f));
    }

    public void StartDrift(float hInput, float speedFactor)
    {
        if (drifting||!grounded) return;

        drifting = true;

        rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);

        driftDirection = Mathf.Sign(hInput) > 0 ? 1 : -1;
        if(hInput==0) driftDirection = 0;

        float startYaw = transform.eulerAngles.y;
        driftInitialYaw = startYaw + (initialRoatateAmount * driftDirection)*1.1f;

        Quaternion targetRot = Quaternion.Euler(0f, driftInitialYaw, 0f);

        rb.MoveRotation(targetRot);
    }

    public void Drift()
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
        if (!im.GetDrifting()||forwardSpeed<driftSpeedThreshold||driftDirection==0)
        {
            exitDrift();
            return;
        }
        

        float currentYaw = transform.eulerAngles.y;

        // Signed difference from starting angle (-180 to 180)
        float deltaFromStart = Mathf.DeltaAngle(driftInitialYaw, currentYaw);

        // If drifting right, prevent rotating back left past start
        if (driftDirection == 1 && deltaFromStart < 0f)
        {
            float clampedYaw = driftInitialYaw;
            rb.MoveRotation(Quaternion.Euler(0f, clampedYaw, 0f));
            rb.angularVelocity = Vector3.zero;
            LockWheels();

        }

        // If drifting left, prevent rotating back right past start
        if (driftDirection == -1 && deltaFromStart > 0f)
        {
            float clampedYaw = driftInitialYaw;
            rb.MoveRotation(Quaternion.Euler(0f, clampedYaw, 0f));
            rb.angularVelocity = Vector3.zero;
            LockWheels();
        }
    }

    public void LockWheels()
    {
        foreach(var wheel in wheels)
        {
            if (wheel.steerable)
            {
                wheel.wheelCollider.steerAngle = 0f;
            }
        }
    }

    public void exitDrift()
    {
        drifting = false;
    }

    public IEnumerator DriftHopFallSpeed(float slerpLength)
    {
        float elapsedTime = 0f;
        while (elapsedTime < slerpLength)
        {
            rb.AddForce(new Vector3(0, -9.81f, 0f), ForceMode.Acceleration);
            yield return new WaitForFixedUpdate();
            elapsedTime += Time.fixedDeltaTime;
        }
    }

    public IEnumerator RotationalSlerpCoroutine(Quaternion targetRotation, float slerpLength)
    {
        float elapsedTime = 0f;
        while (elapsedTime < slerpLength)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsedTime / slerpLength);
            yield return new WaitForFixedUpdate();
            elapsedTime += Time.fixedDeltaTime;
        }
    }
}
