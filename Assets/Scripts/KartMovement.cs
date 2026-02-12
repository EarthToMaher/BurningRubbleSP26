//using System.Numerics;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class KartMovement : MonoBehaviour
{
    // GameManager declaration please do not remove
    [SerializeField] private GameManager GameManager;
    //bool wasAccelerating = false;

    //input actions 
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction reverseAction;
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction driftAction;

    //serialized values
    [SerializeField] private float defaultMaxSpeed; //the max speed the kart can reach
    [SerializeField] private float driftMaxSpeedReducer; //this value is subtracted from defaultMaxSpeed during a drift
    [SerializeField] private float accelerationMultiplier; //acceleration while moving forward
    [SerializeField] private float brakingMultiplier; //how much braking affects speed
    [SerializeField] private float reverseMultiplier = 15; //acceleration while reversing
    [SerializeField] private float turnSpeed; //the speed of a generic turn
    [SerializeField] private float driftSpeed; //the speed of a drift turn
    [SerializeField] private float minDriftAngle; //the min drift angle when player tightens drift (joystick held in drift direction)
    [SerializeField] private float defaultDriftAngle; //the drift angle when joystick is not held in a direction
    [SerializeField] private float maxDriftAngle; //the max angle when player widens drift (joystick held opposite to drift direction)
    [SerializeField] private float driftAngleAdjuster; //how much drift angle changes in a frame based on input
    [SerializeField] private float boostIncrementor; //how much the max speed increases for the start boost and drift boost
    [SerializeField] private float driftBoostIntensity; //length of the drift boost (seconds)

    private float hoverOffset = 1.48f;   // desired height above ground
    private float correctionForce = 1000f;  // how strong to push down
    [SerializeField] private LayerMask groundMask;        // assign your track layer here
    [SerializeField] private Countdown countdown;

    //updating movement values
    private Vector2 moveDirection;
    private float currAcceleration;
    private float currReverse;
    private float currBraking;
    private float currMaxSpeed;
    private float driftDirection;

    //rigidbody reference
    private Rigidbody rb;

    //booleans
    private bool isDrifting;
    private bool lowCOMActive;
    private bool startBoostActive = false;
    private bool driftBoostActive = false;

    //currently doesn't do anything, but here to handle situations in the future and there so I can put it in RubbleBoost for now
    private bool canMove = true;
    private bool interpolating = false;
    private Vector3? boostTargetDirection;
    private float rotationSpeed = 720f;

    public float rubbleAngle = 135f;

    private void Awake()
    {
        //Debug.Log("This is Kart Movement");
        GameManager = GameObject.FindFirstObjectByType<GameManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        GameManager.AudioManager.PlayCategoryOnce("EngineLoop");
        if (currMaxSpeed <= 0) currMaxSpeed = 1f;
        //assign input action references
        moveAction = playerInput.actions["Move"];
        reverseAction = playerInput.actions["Reverse"];
        accelerateAction = playerInput.actions["Accelerate"];
        brakeAction = playerInput.actions["Brake"];
        driftAction = playerInput.actions["Drift"];

        //assign rigidbody and fix max angular velocity for drift
        rb = this.gameObject.GetComponent<Rigidbody>();
        rb.maxAngularVelocity = defaultDriftAngle;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Current Speed is: " + currAcceleration);

        // Audio Input
        //bool isAccelerating = currAcceleration > 0;
        
        GameManager.AudioManager.UpdatePitch(GameManager.AudioManager._updatePitch._name, 1f, 1.8f, currAcceleration / currMaxSpeed);

        // read in move input
        currAcceleration = accelerateAction.ReadValue<float>();
        Debug.Log("TEST ACCELERATION: " + currAcceleration);
        currAcceleration *= accelerationMultiplier;
        //Debug.Log("Acceleration: " + currAcceleration);
        currReverse = reverseAction.ReadValue<float>();
        currReverse *= reverseMultiplier;
        if(currReverse == 0 || currAcceleration != 0)
        {
            //read in normal movement input while accelerating
            moveDirection = moveAction.ReadValue<Vector2>().normalized;
        }
        else
        {
            //read in flipped movement input while reversing
            moveDirection = -moveAction.ReadValue<Vector2>().normalized;
        }
        //Debug.Log("Reverse: " + currReverse);
        currBraking = brakeAction.ReadValue<float>();
        currBraking *= brakingMultiplier;

        //determine whether kart is drifting
        if (driftAction.WasPressedThisFrame() && moveDirection.x != 0 && currAcceleration != 0)
        {
            isDrifting = true;
            rb.constraints = RigidbodyConstraints.None;
            Debug.Log("COM: " + rb.centerOfMass);
            driftDirection = moveDirection.x;
            //Debug.Log(driftDirection);
            Debug.Log("started drifting");
        }

        if (isDrifting && (!driftAction.IsPressed() || currAcceleration == 0))
        {
            isDrifting = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            if (!driftBoostActive)
            {
                StartCoroutine(DriftBoost(driftBoostIntensity));
            }
            else
            {
                currMaxSpeed = defaultMaxSpeed;
                rb.linearVelocity = transform.forward * defaultMaxSpeed;
            }
        }

        if (interpolating)
        {
            Debug.Log(interpolating);
            Vector3 direction = boostTargetDirection.Value;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
                if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
                {
                    interpolating = false;
                }
            }
        }
        //wasAccelerating = isAccelerating;
    }

    void FixedUpdate()
    { 
        //check that countdown is not running before allowing movement logic to run
        if(!countdown.GetActive())
        {
            // steering and drifting
            if (isDrifting)
            {
                //drifting
                //decrease max speed
                currMaxSpeed = defaultMaxSpeed - driftMaxSpeedReducer;

                //adjust drift angle based on input
                if ((driftDirection < 0 && moveDirection.x > 0) || (driftDirection > 0 && moveDirection.x < 0))
                {
                    //widening drift
                    if (rb.maxAngularVelocity < maxDriftAngle)
                    {
                        rb.maxAngularVelocity = Mathf.Clamp(rb.maxAngularVelocity - driftAngleAdjuster, minDriftAngle, maxDriftAngle);
                    }
                    //Debug.Log("Widening: " + rb.maxAngularVelocity);
                }
                else if ((driftDirection < 0 && moveDirection.x < 0) || (driftDirection > 0 && moveDirection.x > 0))
                {
                    //tightening drift
                    if (rb.maxAngularVelocity > minDriftAngle)
                    {
                        rb.maxAngularVelocity = Mathf.Clamp(rb.maxAngularVelocity + (driftAngleAdjuster * 2), minDriftAngle, maxDriftAngle);
                    }
                    //Debug.Log("Tightening: " + rb.maxAngularVelocity);
                }
                else
                {
                    //standard drift, no adjustment
                    if (rb.maxAngularVelocity < defaultDriftAngle)
                    {
                        rb.maxAngularVelocity += driftAngleAdjuster;
                    }
                    else
                    {
                        rb.maxAngularVelocity -= driftAngleAdjuster;
                    }
                    //Debug.Log("Standard: " + rb.maxAngularVelocity);
                }

                //apply torque to make sliding effect
                rb.AddTorque(Vector3.up * driftDirection * driftSpeed, ForceMode.Acceleration);

                //Correctional Torque
                Vector3 torque = Vector3.Cross(transform.up, Vector3.up);
                rb.AddTorque(torque * 10000, ForceMode.Acceleration);
            }
            else
            {
                rb.maxAngularVelocity = defaultDriftAngle;
                //regular steering
                //set max speed to default
                currMaxSpeed = defaultMaxSpeed;

                //kart turn (no drift, not accounting for current speed of kart)
                //Quaternion turnValue = Quaternion.Euler(0f, moveDirection.x * turnSpeed, 0f);

                //kart turn with speed factor (based on current speed of kart)
                //float speedFactor = rb.linearVelocity.magnitude / currMaxSpeed;
                //Quaternion turnValue = Quaternion.Euler(0f, moveDirection.x * turnSpeed * speedFactor, 0f);

                //determine current speed of kart and how much to turn
                float speedFactor = Mathf.Clamp(rb.linearVelocity.magnitude / currMaxSpeed, 0f, 2f);
                Quaternion turnValue = Quaternion.Euler(0f, moveDirection.x * turnSpeed * speedFactor, 0f);

                rb.MoveRotation(rb.rotation * turnValue);

                // eliminate sideways velocity resulting from steering
                Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
                localVel.x = 0;

                // convert to world velocity and apply to kart's rigidbody
                rb.linearVelocity = transform.TransformDirection(localVel);
            }

            //Debug.Log(currReverse);
            // kart acceleration
            if (currBraking != 0 && !isDrifting)
            {
                // Step 1: Get current velocity in local space
                Vector3 localVelocity = rb.transform.InverseTransformDirection(rb.linearVelocity);

                // Step 2: Modify the local Z velocity
                localVelocity.z = Mathf.Abs(localVelocity.z) - currBraking;
                if (localVelocity.z < 0) localVelocity.z = 0;

                // Step 3: Convert modified local velocity back to world space
                rb.linearVelocity = rb.transform.TransformDirection(localVelocity);
            }
            else if (currAcceleration == 0 && !isDrifting && !startBoostActive)
            {
                //add reverse acceleration
                rb.AddRelativeForce(new Vector3(0f, 0f, -1f) * currReverse, ForceMode.Acceleration);
            }
            else if(!startBoostActive)
            {
                //add forward acceleration
                if (rb.linearVelocity.magnitude < currMaxSpeed) rb.AddRelativeForce(new Vector3(0f, 0f, 1f) * currAcceleration, ForceMode.Acceleration);
            }

            // caps acceleration to maxSpeed
            if (rb.linearVelocity.magnitude > currMaxSpeed)
            {
                //Debug.Log("speed: " + rb.linearVelocity.magnitude);

                //float currentSpeed = rb.linearVelocity.magnitude;
                //currentSpeed = Mathf.Clamp(currentSpeed-acc)
                //rb.linearVelocity = rb.linearVelocity.normalized * currMaxSpeed;
                //Debug.Log("speed: " + rb.linearVelocity.magnitude);
            }
        }
    }

    //input functions 
/*    public void OnAccelerate(InputValue value)
    {
        currAcceleration = value.Get<float>();
        Debug.Log("TEST ACCELERATION: " + currAcceleration);
    }*/

    //Temporary Check for destruction, should probably be changed to a spherecast check
    private void OnTriggerStay(Collider other)
    {
        I_Destructible destructible = other.gameObject.GetComponent<I_Destructible>();
        if (destructible != null) destructible.DestroyMe(this.gameObject, this.gameObject);
    }

    public void SetVelocity(Vector3 velocity) { rb.linearVelocity = velocity; }

    public void ResetVelocity() { SetVelocity(Vector3.zero); }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<KartMovement>() != null)
        {
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), collision.gameObject.GetComponent<Collider>());
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Collided: " + collision.gameObject);
        if(!collision.gameObject.CompareTag("Ground"))
        {
            //Debug.Log("Adjusted rotation for wall collision");
            rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            lowCOMActive = true;
        }
    }

    /*    private void OnCollisionExit(Collision collision)
        {
            //Debug.Log("Collision ended: " + collision.gameObject);
            if(lowCOMActive)
            {
                Debug.Log("Changed COM back");
                //rb.centerOfMass = new Vector3(0f, 0f, 0f);
                lowCOMActive = false;
            }
        }*/

    /// <summary>
    /// Logic for doing a rubble boost
    /// </summary>
    /// <returns></returns>
    /// 
    public IEnumerator StartBoost(float boostLevel)
    {
        Debug.Log("in coroutine");
        startBoostActive = true;
        defaultMaxSpeed += boostIncrementor;
        rb.linearVelocity = transform.forward * defaultMaxSpeed;
        Debug.Log("Velocity expected: " + transform.forward * defaultMaxSpeed);
        Debug.Log("Actual velocity: " + rb.linearVelocity);
        yield return new WaitForSeconds(boostLevel);
        defaultMaxSpeed -= boostIncrementor;
        rb.linearVelocity = transform.forward * defaultMaxSpeed;
        startBoostActive = false;
        Debug.Log("end coroutine");
    }

    public IEnumerator DriftBoost(float boostLevel)
    {
        currMaxSpeed = defaultMaxSpeed;
        defaultMaxSpeed += boostIncrementor;
        rb.linearVelocity = transform.forward * defaultMaxSpeed;
        driftBoostActive = true;
        yield return new WaitForSeconds(boostLevel);
        driftBoostActive = false;
        defaultMaxSpeed -= boostIncrementor;
        rb.linearVelocity = transform.forward * defaultMaxSpeed;
    }
    
    public IEnumerator Boost(float intensity, float exitDirection)
    {
        transform.rotation = Quaternion.Euler(0, exitDirection, 0);
        float storedDefaultMaxSpeed = defaultMaxSpeed;
        defaultMaxSpeed = intensity;
        currMaxSpeed = intensity;
        rb.linearVelocity = intensity * transform.forward;
        yield return new WaitForSeconds(0.5f);
        defaultMaxSpeed = storedDefaultMaxSpeed;
        currMaxSpeed = defaultMaxSpeed;
    }

    public IEnumerator RubbleBoost(float intensity)
    {
        Quaternion startingRotation = transform.rotation;
        float storedDefaultMaxSpeed = defaultMaxSpeed;
        defaultMaxSpeed = intensity;
        currMaxSpeed = intensity;

        Vector2 boostDirection = moveDirection;
        if (boostDirection == Vector2.zero) boostDirection = new Vector2(0, 1f);
        Vector3 localDirection = new Vector3(boostDirection.x, 0f, boostDirection.y);
        Vector3 worldDirection = transform.TransformDirection(localDirection).normalized;
        //boostTargetDirection = worldDirection;

        Vector3 clampedDirection = Vector3.RotateTowards(transform.forward, worldDirection, Mathf.Deg2Rad * rubbleAngle, 0f).normalized;

        boostTargetDirection = clampedDirection;

        interpolating = true;
        yield return new WaitUntil(() => !interpolating);
        rb.linearVelocity = transform.forward * intensity;
        yield return new WaitForSeconds(1f);
        defaultMaxSpeed = storedDefaultMaxSpeed;
        currMaxSpeed = defaultMaxSpeed;
    }

    public bool CanMove() { return canMove; }

    public float GetAccelerateValue() { return currAcceleration; }
}




// old code we could need later can go here
// prevent reversing on forward movement
/*  if (localVel.z < 0f && moveDirection.y > 0f)
    {
        Debug.Log("Flip movement direction");
        moveDirection.y *= -1;
    }*/

/*if(Mathf.Abs(driftAngle) > defaultDriftAngle)
{
    //stop further torque from being added
    torqueShouldApply = false;

    //clamp the kart's rotation to stop endless spinning
    //float clampedDriftAngle = Mathf.Clamp(driftAngle, -defaultDriftAngle, defaultDriftAngle);
    //Quaternion targetKartRotation = Quaternion.LookRotation(Quaternion.AngleAxis(clampedDriftAngle, Vector3.up) * velocityDirection);
    //rb.MoveRotation(targetKartRotation);
    Debug.Log("Clamp active");
}*/
//calculate kart's angle
/*Vector3 velocityDirection = rb.linearVelocity.normalized;
float driftAngle = Vector3.SignedAngle(velocityDirection, transform.forward, Vector3.up);*/

/*        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.0f, groundMask))
        {
            float diff = (hit.distance - hoverOffset);
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.green);
            Debug.Log("raycast hit");
            Debug.Log("dist: " + hit.distance);

            // If too high, push down gently
            if (diff > 0.001f)
                rb.AddForce(-Vector3.up * diff * correctionForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }*/
//Debug.Log("Velocity: " + rb.linearVelocity.magnitude);