using UnityEngine;
using System.Collections.Generic;
/*
Script for handling the reenabling of our destroyed objects
Written By: Matthew Maher
Last Updated: 11/05/2025
ChatGPT was used to discuss ways of optimizing
*/
public class ReenableManager : MonoBehaviour
{
    public bool oneMesh;
    //Private class, containing our DestructibleBlock and the earliest time it can respawn
    private class Respawnable
    {
        public DestructibleBlock obj;
        public float eligibleTime;
        public Respawnable(DestructibleBlock o, float t)
        {
            obj = o;
            eligibleTime = t;
        }
    }

    private class OneMeshRespawnable
    {
        public GameObject obj;
        public int xCoord, yCoord, zCoord;
        public byte previousVal;
        public float eligibleTime;
        public OneMeshRespawnable(GameObject o, float t,int x, int y, int z, byte val)
        {
            obj = o;
            eligibleTime = t;
            previousVal = val;
            xCoord = x;
            yCoord = y;
            zCoord = z;

        }
    }

    [Header("Optimization Settings")]
    [Tooltip("How often the reenable check runs in seconds")]
    [SerializeField] private float checkRate = 3f;
    [Tooltip("The maximum # of items it will respawn. Anything less than 1 will respawn all")]
    [SerializeField] private int maxRespawnAmount = 5;
    [Tooltip("The minimum amount of time an object must wait before being respawned")]
    [SerializeField] private float minWaitTime = 5f;

    //Instance variables, handled dynamically
    private readonly List<Respawnable> pending = new();
    private readonly List<OneMeshRespawnable> pendingOneMesh = new();
    private float nextCheckTime;

    //Starts our ReenableClock with our specified parameters
    private void Start()
    {
        InvokeRepeating("ReenableClock", 0, checkRate);
    }

    /// <summary>
    /// Function to check if anything needs to be reenabled
    /// </summary>
    public void ReenableClock()
    {
        if (Time.time < nextCheckTime) return; //Stop running if there is nothing to be reenabled

        // Re-enable all that are ready
        float now = Time.time;
        int amountEnabled = 1; //Set to 1 so that way I can just do a greater than check instead of greater than or equal to
        if (oneMesh)
        {
            Debug.Log("In one mesh");
            Debug.Log(pendingOneMesh.Count);
            if (pendingOneMesh.Count == 0) return; //Stop running if the list is empty
            Debug.Log("Past return");
            OneMeshRespawnable meshRespawnable = null;
            DestructibleMesh dm = null;
            for (int i = pendingOneMesh.Count - 1; i >= 0; i--)
            {
                Debug.Log("In loop");
                if (pendingOneMesh[i].eligibleTime <= now)
                {
                    //Debug.Log(pendingOneMesh[i].obj.transform.GetChild(0).gameObject.GetComponent<DestructibleMesh> == null);
                    meshRespawnable = pendingOneMesh[i];
                    dm = meshRespawnable.obj.transform.GetChild(0).gameObject.GetComponent<DestructibleMesh>();
                    //Debug.Log("IS this TRUE???? " + meshRespawnable.obj.transform.GetChild(0).gameObject.GetComponent<DestructibleMesh>); //We now know that its a null object meaning that it thinks its nothing for some reason
                    if (dm != null){
                        //Debug.Log("Before Value Change: " + meshRespawnable.obj.transform.GetChild(0).gameObject.GetComponent<DestructibleMesh>.voxelData[meshRespawnable.xCoord, meshRespawnable.yCoord, meshRespawnable.zCoord]);
                        dm.voxelData[meshRespawnable.xCoord, meshRespawnable.yCoord, meshRespawnable.zCoord] = meshRespawnable.previousVal; //Turns it back to 1
                        //Debug.Log("After Value Change: " + meshRespawnable.obj.transform.GetChild(0).gameObject.GetComponent<DestructibleMesh>.voxelData[meshRespawnable.xCoord, meshRespawnable.yCoord, meshRespawnable.zCoord]);
                    }
                    pendingOneMesh.RemoveAt(i);
                    if (maxRespawnAmount < 1) continue;
                    amountEnabled++;
                    if (amountEnabled > maxRespawnAmount) break;
                }
            }
            if(meshRespawnable!=null)dm.RepairMe();
            if (pendingOneMesh.Count == 0) nextCheckTime = float.MaxValue;
            else nextCheckTime = pendingOneMesh[0].eligibleTime;
        }
        else
        {
             if (pending.Count == 0) return; //Stop running if the list is empty
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                if (pending[i].eligibleTime <= now)
                {
                    if (pending[i].obj) pending[i].obj.RepairMe();
                    pending.RemoveAt(i);
                    if (maxRespawnAmount < 1) continue; //If our maxRespawnAmount is less than 1, reenable as many as needed
                    amountEnabled++;
                    if (amountEnabled > maxRespawnAmount) break;
                }
                else break; //Since these are all waiting the same time, if we reach one in the list that hasn't reached its wait time yet we can break
            }

            if (pending.Count == 0) nextCheckTime = float.MaxValue; //If there is nothing in the list, sets our nextCheck to be as late as possible
            else nextCheckTime = pending[0].eligibleTime; //Updates our next check time
        }
    }

    /// <summary>
    /// Adds a new Respawnable to the list with the specified DestructibleBlock object
    /// </summary>
    /// <param name="obj">DestructibleBlock that got destroyed</param>
    public void AddToBatch(DestructibleBlock obj)
    {
        pending.Add(new Respawnable(obj, Time.time + minWaitTime)); //Adds the Respawnable with obj and the time it can reenable
        if (pending.Count == 1 || Time.time + minWaitTime < nextCheckTime)
            nextCheckTime = Time.time + minWaitTime; //Updates the next check time relative to this object if its the only object or if its reenable time is less than the next check time
    }

    public void AddToBatchOneMesh(GameObject obj, Vector3 coords, byte val)
    {
        pendingOneMesh.Add(new OneMeshRespawnable(obj, Time.time + minWaitTime, Mathf.FloorToInt(coords.x), Mathf.FloorToInt(coords.y), Mathf.FloorToInt(coords.z), val));
        if (pendingOneMesh.Count == 1 || Time.time + minWaitTime < nextCheckTime) nextCheckTime = Time.time + minWaitTime;
    }

    //Debug function that destroys all DestructibleBlocks in the scene
    [ContextMenu("DestroyAll")]
    public void DestroyAll()
    {
        DestructibleBlock[] blocks = FindObjectsByType<DestructibleBlock>(FindObjectsSortMode.None);
        foreach (DestructibleBlock block in blocks) block.DestroyMe(this.gameObject, this.gameObject);
    }
}
