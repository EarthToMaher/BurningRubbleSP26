using TMPro;
using UnityEngine;

public class CheckpointDetection : MonoBehaviour
{
    public int _lapCount = 1;
    public int _checkpointCount = 0;
    public int _currCheckpoint = 0;
    public int _checkpointRemaining = 0;

    public LapManager _lapManager;

    public GameObject _txtCheckpoint;
    public GameObject _checkpointsRemaining;
    public GameObject _txtLapCount;

    // Update is called once per frame
    void Update()
    {
        if(_lapManager==null) _lapManager = FindFirstObjectByType<LapManager>();

        if (_lapManager == null) return;
        _txtCheckpoint.GetComponent<TextMeshProUGUI>().text = "Current Checkpoint: " + _currCheckpoint;
        _txtLapCount.GetComponent<TextMeshProUGUI>().text = ("Lap: " + _lapCount + "/3");

        _checkpointsRemaining.GetComponent<TextMeshProUGUI>().text = "Remaining Checkpoints " + _checkpointRemaining + "/" + _lapManager.RequirementReturn();
    }
}
