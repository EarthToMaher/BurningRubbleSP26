using UnityEngine;

public class PlayerCamControl : MonoBehaviour
{
    [SerializeField] private Camera _playerCam;
    [SerializeField] private float _maxFOV;
    [SerializeField] private float _currentFOV;
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
        //CamSpeedBoost(_currentFOV);
    }


    public void CamSpeedBoost(float currentFOV)
    {
        Mathf.Clamp(Mathf.Lerp(currentFOV, currentFOV + (currentFOV * .35f), 1), 60, 90);
    }

    [ContextMenu("CamSpeedBoostTest")]
    public void CampSpeedBoostTest()
    {
        Debug.Log("SpeedBoostTest called");
        //Debug.Log("Lerp value: " + ((60 * .35f) + 60));
        //Mathf.Clamp(Mathf.Lerp(60, ((60 * .35f) + 60), 1), 60, 90);
        Mathf.Lerp(60, ((60 * .35f) + 60), 1);
    }
}
