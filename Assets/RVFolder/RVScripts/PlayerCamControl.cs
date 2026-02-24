using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamControl : MonoBehaviour
{
    // TODO
    /*
     * General Camera movement: Needs to follow the player slightly lagging behind the player.
     * Speed Value passthrough: Utilizing the kart's movement, this should determine how far behind the Camera should be.
     * Consensus: These two can honestly be linked together. In order for the camera to follow behind the player, it needs to also *catch up*
     * with the player. I can easily do this when the kart is actually finished for more smooth testing.
     */

    // Core objects
    [SerializeField] private Camera _playerCam;
    [SerializeField] private GameObject _kart;
    // Logic for FOV boost
    [SerializeField] private float _maxFOV;
    [SerializeField] private float _currentFOV;
    [SerializeField] private float _boostPauseMiliseconds = 30f;
    // Logic variables for player tracking
    private Vector3 _centerPoint;
    private float radius = 5f;
    private float _speed = 2f;
    private float maxAngle = 45f;
    
    private InputManager _inputManager;

    private bool _isBoosting = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playerCam = transform.parent.GetComponentInChildren<Camera>();
        _kart = this.gameObject;
        _currentFOV = transform.parent.GetComponentInChildren<Camera>().fieldOfView;
        _inputManager = this.GetComponent<InputManager>();
    }
    void Start()
    {
        PlayerCamFollowEngage();  
    }

    void Update()
    {
        _centerPoint = GetComponentInChildren<Transform>().position;
        _playerCam.transform.LookAt(_centerPoint);
        PlayerCamFollowEngage();
    }

    public void PlayerCamFollowEngage()
    {
        StartCoroutine(CamPlayerFollow());
    }

    IEnumerator CamPlayerFollow()
    {
        while (true)
        {
            //float acceleration = _inputManager.GetAcceleration() - _inputManager.GetReverse();

            // Create an offset Vector between the Camera's position and the center point.
            Vector3 offset = _playerCam.transform.position - _centerPoint;
            
            // Camera relative to the kart
            Vector3 kartForward = _kart.transform.forward;
            Vector3 toCamera = _playerCam.transform.position - _kart.transform.position;
            
            // Smoothly move toward center point
            Vector3 desiredPosition = Vector3.Lerp(_playerCam.transform.position, _centerPoint, _speed * Time.deltaTime);
            Vector3 newOffset = desiredPosition - _centerPoint;

            // Clamp radius if camera is too close
            if (newOffset.magnitude < radius)
            {
                newOffset = newOffset.normalized * radius;
            }

            // Clamp cone behind kart
            float angle = Vector3.Angle(-kartForward, newOffset);

            if (angle > maxAngle)
            {
                // Only rotate direction, keep magnitude (distance)
                Vector3 dir = newOffset.normalized; // current direction
                Vector3 clampedDir = Vector3.RotateTowards(-kartForward, dir, Mathf.Deg2Rad * maxAngle, 0f);
                newOffset = clampedDir * newOffset.magnitude; // preserve distance
            }

            // New position set
            _playerCam.transform.position = _centerPoint + newOffset;

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
