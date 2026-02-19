using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamControl : MonoBehaviour
{

    [SerializeField] private Camera _playerCam;
    [SerializeField] private float _maxFOV;
    [SerializeField] private float _currentFOV;
    [SerializeField] private float _time = 0.0f;
    [SerializeField] private float _boostPauseMiliseconds = 30f;
    //[SerializeField] public float _currentSpeed;

    [SerializeField] private bool _isBoosting = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playerCam = this.GetComponent<Camera>();
        _currentFOV = this.GetComponent<Camera>().fieldOfView;
    }
    void Start()
    {
        CamSpeedBoostEngage();
    }

    // Update is called once per frame
    void Update()
    {
        _currentFOV = this.GetComponent<Camera>().fieldOfView;  
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
