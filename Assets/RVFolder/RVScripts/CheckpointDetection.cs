using TMPro;
using UnityEngine;

public class CheckpointDetection : MonoBehaviour
{
    [Tooltip("Starting Lap")]
    public int _lapCount = 1;
    [Tooltip("Num of checkpoints hit")]
    public int _checkpointCount = 0;
    [Tooltip("Current Checkpoint Index")]
    public int _currCheckpoint = 0;
    [Tooltip("Num of Checkpoints Remaining")]
    public int _checkpointRemaining = 0;

    private int[] checkPointsHit;
    private int arrayIndex = 0;
    public LapManager _lapManager;

    public GameObject _txtCheckpoint;
    public GameObject _checkpointsRemaining;
    public GameObject _txtLapCount;

    void Start()
    {
        ResetArray();
        UpdateUI();
    }

    public void enterCheckpoint(int checkPointPlacement)
    {
        
        if (CheckArrayFor(checkPointPlacement))
        {
            Debug.LogWarning("Hit a new checkpoint!");
            _currCheckpoint = checkPointPlacement;
            _checkpointCount++;
            if (_checkpointRemaining < _lapManager.RequirementReturn())
            {
                _checkpointRemaining++;
                checkPointsHit[arrayIndex] = checkPointPlacement;
                arrayIndex++;
            }
            UpdateUI();
        }
        else Debug.LogWarning("I already hit that checkpoint!");
    }

    public void CompleteLap()
    {
        
        if (_checkpointRemaining >= _lapManager.RequirementReturn())
        {
            Debug.LogWarning("I completed my " + _lapCount +" lap!");
            _lapCount++;
            _checkpointCount = 0;
            _checkpointRemaining = 0;
            ResetArray();
            UpdateUI();
        }
        else Debug.LogWarning("I didn't have enough checkpoints!");
    }

    public void UpdateUI()
    {
        //_txtCheckpoint.GetComponent<TextMeshProUGUI>().text = "Current Checkpoint: " + _currCheckpoint;

        _txtLapCount.GetComponent<TextMeshProUGUI>().text = ("Lap: " + _lapCount + "/3"); //Updates which lap we are on

        _checkpointsRemaining.GetComponent<TextMeshProUGUI>().text = "Remaining Checkpoints " + _checkpointRemaining + "/" + _lapManager.RequirementReturn(); //Updates how many checkpoints we hit.
    }

    public bool CheckArrayFor(int checkPointIndex)
    {
        foreach (int checkpoint in checkPointsHit)
        {
            if (checkpoint==checkPointIndex)return false;
            if(checkpoint==-1) return true;
        }
        return true;
    }

    public void ResetArray()
    {
        checkPointsHit = new int[_lapManager.RequirementReturn()];
        for(int i = 0; i < checkPointsHit.Length; i++) checkPointsHit[i] = -1;
        arrayIndex = 0;
    }

    
}
