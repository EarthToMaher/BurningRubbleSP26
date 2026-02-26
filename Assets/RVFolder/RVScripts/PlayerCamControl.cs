using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamControl : MonoBehaviour
{
    // TODO
    /*
     * Fix Camera starting at world origin
     * Fix Camera Kart jitter
     * Link speed to FOV
     */

    // Core objects
    [SerializeField] private Camera _playerCam;
    [SerializeField] private GameObject _kart;
    // Logic for FOV boost
    [SerializeField] private float _maxFOV;
    [SerializeField] private float _currentFOV;
    [SerializeField] private float _boostPauseMiliseconds = 30f;
    // Logic variables for player tracking & Camera
    private Vector3 _centerPoint;
    private float _radius = 5f;
    private float _speed = 2f;
    private float _maxAngle = 45f;
    [SerializeField] private float _maxDistance = 2f;

    // Input action to get acceleration
    private PlayerInput playerInput;
    private InputAction accelerateAction;
    private float currAcceleration;
    private float defaultFOV = 60f;
    private float targetSpeedPOV = 70f;
    private InputManager _inputManager;

    private bool _isBoosting = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playerCam = transform.parent.GetComponentInChildren<Camera>();
        _kart = this.gameObject;
        _currentFOV = transform.parent.GetComponentInChildren<Camera>().fieldOfView;
        _inputManager = this.GetComponent<InputManager>();

       // Initialize Camera position
       Vector3 startOffset = -_kart.transform.forward * _radius + Vector3.up * 2f;
       _centerPoint = GetComponentInChildren<Transform>().position;
       _playerCam.transform.position = _centerPoint + startOffset;
        PlayerCamFollowEngage();

        //
        playerInput = GetComponentInChildren<PlayerInput>();
        accelerateAction = playerInput.actions["Accelerate"];
    }
    void Start()
    {

    }

    void Update()
    {
        _centerPoint = GetComponentInChildren<Transform>().position;

        PlayerCamFollowEngage();
    }

    private void LateUpdate()
    {
        // Get the direction from the camera to the kart
        Vector3 direction = _centerPoint - _playerCam.transform.position;

        // Avoid zero-length vectors (just in case)
        if (direction.sqrMagnitude > 0.0001f)
        {
            // Get the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate the camera toward the target rotation
            _playerCam.transform.rotation = Quaternion.Slerp(
            _playerCam.transform.rotation,
            targetRotation,
            Time.deltaTime * 10f
            );
        }
    }

    public void PlayerCamFollowEngage()
    {
        StartCoroutine(CamPlayerFollow());
    }

    IEnumerator CamPlayerFollow()
    {
        //currAcceleration = accelerateAction.ReadValue<float>();
        //Debug.Log("Current Acceleration: " + currAcceleration);
        Rigidbody rb = _kart.GetComponent<Rigidbody>();
        float currAcceleration = rb.linearVelocity.magnitude;
        Debug.Log(currAcceleration);
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, _kart.transform.forward);
        if (forwardSpeed > 0f)
        {
            _playerCam.fieldOfView = Mathf.Lerp(defaultFOV, targetSpeedPOV, currAcceleration);
        }
        while (true)
        {
            // Camera relative to the kart
            Vector3 kartForward = _kart.transform.forward;
            Vector3 desiredPosition = Vector3.Lerp(_playerCam.transform.position, _centerPoint, _speed * Time.deltaTime);

            Vector3 newOffset = desiredPosition - _centerPoint;

            // Clamp radius if camera is too close
            if (newOffset.magnitude < _radius)
            {
                newOffset = newOffset.normalized * _radius;
            }

            // Clamp cone behind kart
            float angle = Vector3.Angle(-kartForward, newOffset);

            if (angle > _maxAngle)
            {
                // Get the direction from the kart to the camera
                Vector3 currentDir = newOffset.normalized;

                // Desired maximum allowed direction (on cone edge)
                Vector3 maxDir = Vector3.RotateTowards(
                    -kartForward,
                    currentDir,
                    Mathf.Deg2Rad * _maxAngle,
                    0f
                );

                newOffset = maxDir * newOffset.magnitude;
            }

            // Final target
            Vector3 finalPosition = _centerPoint + newOffset;

            // Keep camera min distance from ground
            RaycastHit hit;
            if (Physics.Raycast(finalPosition, _playerCam.transform.position = Vector3.down, out hit, 100f))
            {
                float minAllowedY = hit.point.y + _maxDistance;

                if (finalPosition.y < minAllowedY)
                {
                    finalPosition.y = minAllowedY;
                }
            }

            // Set final destination
            _playerCam.transform.position = finalPosition;
            
            // End of IEnumerator
            yield return null;
        }
        
    }

    // Fully functional
    public void CamSpeedBoostEngage()
    {
        Debug.Log("CamSpeedBoostEngage called");
        if (_isBoosting)
        {
            Debug.Log("Engaging Coroutine");
            StartCoroutine(CamSpeedBoostRoutine());
        }
    }


    // Fully functional
    IEnumerator CamSpeedBoostRoutine()
    {
        //Debug.Log("called");
        float defaultFOV = _playerCam.fieldOfView;
        float targetFOV = Mathf.Clamp(defaultFOV * 1.35f, 60f, 90f);

        // Time called everytime the function activates
        float duration = .25f;
        float time = 0f;

        //Debug.Log("Moving forward");

        // Camera pullback logic 
        while (time < duration)
        {
            Debug.Log("Within While loop");
            _playerCam.fieldOfView = Mathf.Lerp(defaultFOV, targetFOV, time / duration);
            time += Time.deltaTime;
            Debug.Log("Time = " + time);
            yield return null;
        }

        yield return new WaitForSeconds(_boostPauseMiliseconds * Time.deltaTime);

        // Reset the timer & set new duration
        time = 0f;
        duration = 1.25f;

        // Camera return logic
        while (time < duration)
        {
            _playerCam.fieldOfView = Mathf.Lerp(targetFOV, defaultFOV, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        _isBoosting = false;
        
      }
}
