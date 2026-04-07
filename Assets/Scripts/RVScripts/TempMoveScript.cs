using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TempMoveScript : MonoBehaviour
{
    public int _lapCount = 1;
    public int _checkpointCount = 0;
    public int _currCheckpoint = 0;

    public GameObject _txtCheckpoint;
    public GameObject _txtLapCount;

    // Everything below this honestly is just a temp movement script. Above it will be values associated to Lap Checking.
    private PlayerInput playerInput;
    private InputAction moveAction;

    public float moveSpeed = 5f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];

    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y);
        transform.position += move * moveSpeed * Time.deltaTime;

        _txtCheckpoint.GetComponent<TextMeshProUGUI>().text = "Current Checkpoint: " + _checkpointCount;
    }

    public void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Entered Trigger");
        if (other.transform.parent.tag.Equals("Lap"))
        {
            Debug.Log("Triggered Lap");
            if (_lapCount <= 2)
            {
                _lapCount++;
                Debug.Log("Lap Count is: " + _lapCount);
                _txtLapCount.GetComponent<TextMeshProUGUI>().text = "Lap: " + _lapCount + "/3";
            }
        }

    }
}