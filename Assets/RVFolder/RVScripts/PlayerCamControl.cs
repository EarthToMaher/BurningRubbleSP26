using UnityEngine;

public class PlayerCamControl : MonoBehaviour
{
    [SerializeField] private Camera _playerCam;
    [SerializeField] private float _maxFOV;
    [SerializeField] private float _currentFOV;
    [SerializeField] private float _time = 0.0f;
    //[SerializeField] public float _currentSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playerCam = this.GetComponent<Camera>();
        _currentFOV = this.GetComponent<Camera>().fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        _currentFOV = this.GetComponent<Camera>().fieldOfView;
        CamSpeedBoost(_currentFOV);
        //CamSpeedBoost(_currentFOV);
    }


    public void CamSpeedBoost(float currentFOV)
    {
        float _defaultFOV = currentFOV;
        bool reverseFOV = false;
        //Mathf.Clamp(Mathf.Lerp(currentFOV, currentFOV + (currentFOV * .35f), 1), 60, 90);
        // Time is called everytime this function activates.
        _time = 0.0f;
        _time += 5f * Time.deltaTime;

        Debug.Log("Moving forward");
        if (!reverseFOV)
        {
            Debug.Log(_time);
            // While utilizing time, lerp between our currentFOV passed in and expand it by a calculated amount. Clamped at a minimum of 60 and max of 90.
            _playerCam.fieldOfView = Mathf.Clamp(Mathf.Lerp(currentFOV, ((currentFOV * .35f) + currentFOV), _time), 60, 90);
            if (currentFOV >= 80) 
            {
                Debug.Log("Triggered change in FOV");
                reverseFOV = true;
            }
        } else if (reverseFOV)
        {
            Debug.Log("Moving backwards");
            _playerCam.fieldOfView = Mathf.Clamp(Mathf.Lerp(currentFOV, _defaultFOV, _time), 60, 90);
        }
    }

    [ContextMenu("CamSpeedBoostTest")]
    public void CamSpeedBoostTest()
    {
        Debug.Log("SpeedBoostTest called");
        //Debug.Log("Lerp value: " + ((60 * .35f) + 60));
        //Mathf.Clamp(Mathf.Lerp(60, ((60 * .35f) + 60), 1), 60, 90);
        //_time += 0.5f * Time.deltaTime;
        Debug.Log(_time);
        _playerCam.fieldOfView = Mathf.Lerp(60, ((60 * .35f) + 60), _time);
    }
}
