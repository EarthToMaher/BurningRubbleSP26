using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using MarchingCubesProject;

//A lot of this script used a ChatGPT script as a base (mainly so I didn't have to deal with typing out as much math)


public class DestructibleMesh : MonoBehaviour, I_Destructible
{
    public Vector3[,,] voxelPositions;
    public ReenableManager rm;
    public byte[,,] voxelData;
    public GridPiece parentGridPiece;

    public int size = 16;       // number of voxels per axis
    public float voxelSize = 1f;
    public int hp = 5;
    public int rubble = 5;

    public Vector3 hitRadius = new Vector3(1.1f,1,1.5f); // x,y,z radius for destruction

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Example marchingCubesScript;

    void Awake()
    {
        rm = FindFirstObjectByType<ReenableManager>();
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        marchingCubesScript = FindFirstObjectByType<Example>();

        if (parentGridPiece == null)
            parentGridPiece = GetComponentInParent<GridPiece>();

        if(parentGridPiece != null){  
            voxelData = parentGridPiece.GetVoxelData();
            voxelPositions = parentGridPiece.GetVoxelPositions();
        }else{
            voxelData = marchingCubesScript.worldVoxelization.voxelData;
            voxelPositions = marchingCubesScript.worldVoxelization.gridLocations;
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collision detected with " + other.gameObject.name);
        DestroyMe(other.gameObject, other.gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        //Debug.Log("Collision stay detected with " + other.gameObject.name);
        DestroyMe(other.gameObject, other.gameObject);
    }

    public void DestroyMe(GameObject instigator, GameObject cause)
    {
        Vector3 hitpoint = meshCollider.ClosestPoint(cause.transform.position);
        Debug.Log("Hitpoint: " + hitpoint);
        int numDestroyed = ApplyHit(hitpoint);
        I_Damageable damageable = cause.GetComponent<I_Damageable>();
        if (damageable != null) damageable.TakeDamage(hp*numDestroyed);
        RubbleMeter rm = instigator.GetComponent<RubbleMeter>();
        if (rm != null) rm.GainRubble(rubble*numDestroyed);
    }

    /*int ApplyHit(Vector3 worldPoint)
    {
        int count = 0;
        // Convert world point to local voxel space
        //Debug.Log("World Point: " + worldPoint);
        //Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        //Debug.Log("Local Point: " + localPoint);

        //localPoint.y += 0.1f;
        //localPoint.x += 0.15f;

        //Vector3 min = worldPoint - hitRadius;
        //Vector3 max = worldPoint + hitRadius;
        Vector3 localHit = transform.InverseTransformPoint(worldPoint);
        Vector3 min = localHit - hitRadius;
        Vector3 max = localHit + hitRadius;

        bool modified = false;

        Debug.Log("Voxel Data Size: " + voxelData.GetLength(0) + ", " + voxelData.GetLength(1) + ", " + voxelData.GetLength(2));

        for (int x = 0; x < voxelData.GetLength(0); x++)
            for (int y = 0; y < voxelData.GetLength(1); y++)
                for (int z = 0; z < voxelData.GetLength(2); z++)
                {
                    //Debug.Log("Checking voxel: " + voxelData[x,y,z]);

                    if (voxelData[x, y, z] == 0) continue;

                    Vector3 voxelCenter = voxelPositions[x, y, z];
                    //Debug.Log("Voxel Center: " + voxelCenter);

                    // Check if voxel center is inside the hit bounds //CURRENTLY NOT WORKING PROPERLY BECAUSE THE VOXELS ARE NOT... DOING CORRECT THINGS
                    //Debug.Log("Voxel Center: " + voxelCenter + " Min: " + min + " Max: " + max);
                    if (voxelCenter.x >= min.x && voxelCenter.x <= max.x &&
                        voxelCenter.y >= min.y && voxelCenter.y <= max.y &&
                        voxelCenter.z >= min.z && voxelCenter.z <= max.z)
                    {
                        Vector3 coords = new Vector3(x, y, z);
                        rm.AddToBatchOneMesh(this.transform.parent.gameObject,coords,voxelData[x,y,z]); //Check that all the values are correct
                        voxelData[x, y, z] = 0;
                        //Debug.Log("Voxel destroyed");
                        count++;
                        modified = true;
                    }
                }

        if (modified)
        {
            Debug.Log("Rebuilding mesh after destruction");
            marchingCubesScript.RegenerateMarchingCubesMesh(voxelData, voxelPositions);//RebuildMesh();
            modified = false;
        }

        return count;
    }*/

    int ApplyHit(Vector3 worldPoint)
    {
        int count = 0;

        // Convert hit point from world space → this chunk's local voxel space
        // Vector3 localHit = transform.InverseTransformPoint(worldPoint);
        // Vector3 min = localHit - hitRadius;
        // Vector3 max = localHit + hitRadius;

        Vector3 min = worldPoint - hitRadius;
        Vector3 max = worldPoint + hitRadius;

        bool modified = false;

        for (int x = 0; x < voxelData.GetLength(0); x++)
            for (int y = 0; y < voxelData.GetLength(1); y++)
                for (int z = 0; z < voxelData.GetLength(2); z++)
                {
                    if (voxelData[x, y, z] == 0) continue;

                    Vector3 voxelCenter = voxelPositions[x, y, z];

                    // Now in same coordinate space
                    if (voxelCenter.x >= min.x && voxelCenter.x <= max.x &&
                        voxelCenter.y >= min.y && voxelCenter.y <= max.y &&
                        voxelCenter.z >= min.z && voxelCenter.z <= max.z)
                    {
                        rm.AddToBatchOneMesh(
                            this.transform.parent.gameObject,
                            new Vector3(x, y, z),
                            voxelData[x, y, z]);

                        voxelData[x, y, z] = 0;
                        modified = true;
                        count++;
                    }
                }

        if (modified)
        {
            Debug.Log("Rebuilding mesh after destruction");
            marchingCubesScript.RegenerateMarchingCubesMesh(voxelData, voxelPositions);
        }

        return count;
    }


    public void RebuildMesh()
    {
        Debug.Log("Attempted to rebuild mesh");
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        Vector3[] faceDirs =
        {
            Vector3.forward, Vector3.back,
            Vector3.up, Vector3.down,
            Vector3.right, Vector3.left
        };

        Vector3[,] faceVerts = new Vector3[6, 4]
        {
            { new Vector3(-0.5f,-0.5f,0.5f), new Vector3(0.5f,-0.5f,0.5f), new Vector3(0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f) },
            { new Vector3(0.5f,-0.5f,-0.5f), new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(-0.5f,0.5f,-0.5f), new Vector3(0.5f,0.5f,-0.5f) },
            { new Vector3(-0.5f,0.5f,0.5f), new Vector3(0.5f,0.5f,0.5f), new Vector3(0.5f,0.5f,-0.5f), new Vector3(-0.5f,0.5f,-0.5f) },
            { new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,0.5f), new Vector3(-0.5f,-0.5f,0.5f) },
            { new Vector3(0.5f,-0.5f,0.5f), new Vector3(0.5f,-0.5f,-0.5f), new Vector3(0.5f,0.5f,-0.5f), new Vector3(0.5f,0.5f,0.5f) },
            { new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(-0.5f,-0.5f,0.5f), new Vector3(-0.5f,0.5f,0.5f), new Vector3(-0.5f,0.5f,-0.5f) }
        };

        int sx = voxelData.GetLength(0);
        int sy = voxelData.GetLength(1);
        int sz = voxelData.GetLength(2);

        for (int x = 0; x < sx; x++)
            for (int y = 0; y < sy; y++)
                for (int z = 0; z < sz; z++)
                {
                    if (voxelData[x, y, z] == 0) continue;

                    Vector3 center = voxelPositions[x, y, z];

                    for (int f = 0; f < 6; f++)
                    {
                        Vector3Int neighbor = new Vector3Int(x, y, z) + Vector3Int.RoundToInt(faceDirs[f]);
                        bool neighborSolid = false;

                        if (neighbor.x >= 0 && neighbor.x < sx &&
                            neighbor.y >= 0 && neighbor.y < sy &&
                            neighbor.z >= 0 && neighbor.z < sz)
                        {
                            neighborSolid = voxelData[neighbor.x, neighbor.y, neighbor.z] == 1;
                        }

                        if (neighborSolid) continue;

                        int start = verts.Count;
                        for (int i = 0; i < 4; i++)
                            verts.Add(center + faceVerts[f, i] * voxelSize);

                        tris.Add(start + 0);
                        tris.Add(start + 1);
                        tris.Add(start + 2);
                        tris.Add(start + 2);
                        tris.Add(start + 3);
                        tris.Add(start + 0);
                    }
                }

        Mesh newMesh = new Mesh();
        newMesh.indexFormat = IndexFormat.UInt32;
        newMesh.SetVertices(verts);
        newMesh.SetTriangles(tris, 0);
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();

        meshFilter.sharedMesh = newMesh;

        // Update collider
        if (meshCollider)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = newMesh;
        }
    }

    public void RepairMe()
    {
        marchingCubesScript.RegenerateMarchingCubesMesh(voxelData, voxelPositions);
    }
}