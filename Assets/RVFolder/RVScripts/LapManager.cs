using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LapManager : MonoBehaviour
{
    [SerializeField] private Checkpoint[] _checkpointArray;
    [SerializeField] private int _checkpointReqNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Chatgpt generated
        var _scene = SceneManager.GetActiveScene();
        var _rootObjects = _scene.GetRootGameObjects();

        /*_checkpointArray = _rootObjects
            .SelectMany(root => root.GetComponentsInChildren<Checkpoint>(true))
            .OrderBy(c => c.GetCheckpoint())
            .ToArray();*/

        _checkpointArray = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).OrderBy(c=>c.GetCheckpoint()).ToArray();

        // Up to here
        // It was gonna be way more of a headache for me to think of this on my own, so thanks chatgpt
        // You a real one
    }



    public Vector3 SetCheckpointPos(int currCheckpoint)
    {
        return _checkpointArray[currCheckpoint]._checkpointPosition;
    }

    public Quaternion SetCheckpointRot(int currCheckpoint)
    {
        return _checkpointArray[currCheckpoint]._checkpointRotation;
    }

    public int RequirementReturn() 
    {
        return _checkpointReqNum;
    }

    public void DisableHasPassed()
    {
        foreach (var _hasPassede in _checkpointArray) 
        {
            for (int i = 0; i < _checkpointArray.Length; i++)
            {
                _checkpointArray[i]._hasPassed = false;
            }
        }
    }
}
