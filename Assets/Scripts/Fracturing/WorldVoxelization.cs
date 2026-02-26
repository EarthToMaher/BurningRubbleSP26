using UnityEngine;
using System.Collections.Generic;
using MarchingCubesProject;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;


[ExecuteInEditMode]
public class WorldVoxelization : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 1f;
    public Color gridColor = Color.gray;
    public bool includeInactive = false;
    public bool fitToRenderers = true;
    public bool fitToColliders = true;
    Vector3 start;
    Vector3 end;
    public GridPiece[,] arrayOfGridPieces;
    public GridPiece[] listofGridPieces = new GridPiece[10];

    [Header("Layer Settings")]
    public string targetLayerName = "Voxelize";
    public List<GameObject> listOfObjectsToVoxelize = new List<GameObject>();

    [Header("Cube Placement")]
    public GameObject cubePrefab;       // Optional prefab — if null, will use a built-in cube
    public bool clearOldCubes = true;   // Destroys previously placed cubes before spawning new ones
    public string spawnedParentName = "VoxelGrid";
    public GameObject parent;

    private Bounds sceneBounds;
    private int targetLayer;

    //[Header("Grid Locations")]
    public Vector3[,,] gridLocations = new Vector3[0,0,0];
    public byte[,,] voxelData = new byte[0,0,0];
    public List<Voxel> voxels = new List<Voxel>();

    public Example marchingCubesScript;

    public ReenableManager reenableManager;

    void Start(){
        if (!Application.isPlaying)
            return;

        resetSmallGrids();
        GenerateVoxelGrid();
        CutUpLargeGrid();
        foreach(GameObject singleObjectInList in listOfObjectsToVoxelize){
            singleObjectInList.SetActive(false); //turns off the objects after using them to make voxelized mesh
        }
    }

    void OnApplicationQuit()
    {
        DestroyImmediate(parent);
        Debug.Log("Destroyed parent object on application quit.");
    }

    [ContextMenu("Generate Voxel Grid")]

    public void GenerateVoxelGrid()
    {
        if(arrayOfGridPieces == null){
            clearRemainingVoxels();
            targetLayer = LayerMask.NameToLayer(targetLayerName);
            if (targetLayer < 0)
            {
                Debug.LogWarning($"Layer \"{targetLayerName}\" not found. Please create it in the Tags & Layers settings.");
                return;
            }

            UpdateSceneBounds();

            bool conditionalForCreation = false;

            // Optionally clear previously spawned cubes //does not work as far as I can tell
            if (clearOldCubes)
            {
                var oldParent = GameObject.Find(spawnedParentName);
                conditionalForCreation = true;
                if (oldParent != null)
                    DestroyImmediate(oldParent);
            }

            if(conditionalForCreation || parent == null){
                // Create parent to hold cubes
                parent = new GameObject(spawnedParentName);
                parent.transform.position = Vector3.zero;

                start = new Vector3(
                    Mathf.Floor(sceneBounds.min.x / cellSize) * cellSize,
                    Mathf.Floor(sceneBounds.min.y / cellSize) * cellSize,
                    Mathf.Floor(sceneBounds.min.z / cellSize) * cellSize
                );

                end = new Vector3(
                    Mathf.Ceil(sceneBounds.max.x / cellSize) * cellSize,
                    Mathf.Ceil(sceneBounds.max.y / cellSize) * cellSize,
                    Mathf.Ceil(sceneBounds.max.z / cellSize) * cellSize
                );

                // Initialize grid locations array
                gridLocations = new Vector3[Mathf.CeilToInt((end.x - start.x) / cellSize) +2, Mathf.CeilToInt((end.y - start.y) / cellSize) +2, Mathf.CeilToInt((end.z - start.z) / cellSize) +2];
                voxelData = new byte[gridLocations.GetLength(0), gridLocations.GetLength(1), gridLocations.GetLength(2)];
                Debug.Log("Grid size: " + gridLocations.GetLength(0) + " x " + gridLocations.GetLength(1) + " x " + gridLocations.GetLength(2));
                Debug.Log("Start X: " + start.x + " End X: " + end.x + "\nStart Y: " + start.y + " End Y: " + end.y + "\nStart Z: " + start.z + " End Z: " + end.z);


                int count = 0;

                List<MeshCollider> mcList = GetMeshCollidersInLayer();

                int a=0, b=0, c=0;
                // Main loop to fill the grid
                for (float x = start.x; x < end.x + 2; x += cellSize)
                {
                    for (float y = start.y; y < end.y + 2; y += cellSize)
                    {
                        for (float z = start.z; z < end.z + 2; z += cellSize)
                        {

                            Vector3 cellCenter = new Vector3((x + cellSize / 2f) - cellSize, (y + cellSize / 2f) - cellSize, (z + cellSize / 2f) - cellSize);
                            gridLocations[a,b,c] = new Vector3(cellCenter.x, cellCenter.y,  cellCenter.z);

                            if (IsPointInsideMesh(cellCenter, mcList))
                            {
                                GameObject cube;
                                /*
                                if (cubePrefab != null)
                                    cube = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab);
                                else
                                    cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                                cube.transform.position = cellCenter;
                                cube.transform.localScale = Vector3.one * cellSize;
                                cube.transform.SetParent(parent.transform);
                                cube.GetComponent<BoxCollider>().isTrigger = true;
                                cube.AddComponent<DestructibleBlock>();//*/

                                count++;
                                voxelData[a, b, c] = 1; // Mark voxel as occupied
                                //voxels.Add(new Voxel(new Vector3Int(a,b,c), 1)); 
                                //Need to work on changing it from various independent variables to the Voxel class data, but thats a later problem
                            }else{
                                voxelData[a, b, c] = 0; // Mark voxel as empty
                                //voxels.Add(new Voxel(new Vector3Int(a,b,c), 0));
                            }
                            c++;
                        }
                        c = 0;
                        b++;
                    }
                    b = 0;
                    a++;
                }

                marchingCubesScript.GenerateMarchingCubesMeshBig(voxelData, gridLocations);

                Debug.Log($"Generated {count} cubes inside grid fitting \"{targetLayerName}\" objects.");
            }
        }else{
            //int x=0;
            for(int i=0; i<arrayOfGridPieces.GetLength(0); i++){
                for(int j=0; j<arrayOfGridPieces.GetLength(1); j++){
                    Debug.Log("Generating mesh for small grid piece at: " + i + ", " + j);
                    Debug.Log("Edge Case: " + arrayOfGridPieces[i,j].GetEdgeCases());
                    Debug.Log("Grid Piece: " + arrayOfGridPieces[i,j]);
                    //listofGridPieces[x] = arrayOfGridPieces[i,j];
                    //x++;
                    marchingCubesScript.GenerateMarchingCubesMesh(arrayOfGridPieces[i,j]);
                }
            }
        }
    }

    void UpdateSceneBounds()
    {
        bool initialized = false;
        sceneBounds = new Bounds();

        if (fitToRenderers)
        {
            Renderer[] renderers = includeInactive ? FindObjectsOfType<Renderer>(true) : FindObjectsOfType<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r.gameObject.layer != targetLayer) continue;

                if (!initialized)
                {
                    sceneBounds = r.bounds;
                    initialized = true;
                }
                else
                {
                    sceneBounds.Encapsulate(r.bounds);
                }
            }
        }

        if (fitToColliders)
        {
            Collider[] colliders = includeInactive ? FindObjectsOfType<Collider>(true) : FindObjectsOfType<Collider>();
            foreach (Collider c in colliders)
            {
                if (c.gameObject.layer != targetLayer) continue;

                if (!initialized)
                {
                    sceneBounds = c.bounds;
                    initialized = true;
                }
                else
                {
                    sceneBounds.Encapsulate(c.bounds);
                }
            }
        }

        if (!initialized)
        {
            sceneBounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        }
    }

    bool IsPointInsideMesh(Vector3 point, List<MeshCollider> mcList)
    {
        Debug.Log("I EXIST!!!");
        // tiny overlap sphere at the point
        Collider[] overlaps = Physics.OverlapSphere(point, 0.0001f);

        foreach (var c in overlaps)
        {
            Debug.Log("Overlap with " + c.name);
            foreach (var mc in mcList)
            {
                if (c == mc)
                    return true; // definitely inside
            }
        }
        //int count = 0;

        return false;
    }

    public List<MeshCollider> GetMeshCollidersInLayer()
    {
        List<MeshCollider> mcList = new List<MeshCollider>();
        MeshCollider[] colliders = includeInactive ? FindObjectsOfType<MeshCollider>(true) : FindObjectsOfType<MeshCollider>(true);
        foreach (MeshCollider mc in colliders)
        {
            if (mc.gameObject.layer != targetLayer) continue;
            mcList.Add(mc);
            listOfObjectsToVoxelize.Add(mc.gameObject);
        }
        Debug.Log("Found " + mcList.Count + " colliders in layer " + targetLayerName);
        return mcList;
    }

    public void recheckVoxels(){
        /*int a=0, b=0, c=0;
        voxelData = new byte[gridLocations.GetLength(0), gridLocations.GetLength(1), gridLocations.GetLength(2)];
        Debug.Log("VoxelDataLength: " + voxelData.GetLength(0) + ", " + voxelData.GetLength(1) + ", " + voxelData.GetLength(2));
        for (float x = start.x; x < end.x + 2; x += cellSize)
        {
            for (float y = start.y; y < end.y + 2; y += cellSize)
            {
                for (float z = start.z; z < end.z + 2; z += cellSize)
                {
                    //Debug.Log("Rechecking voxel at: " + a + ", " + b + ", " + c);
                    voxelData[a, b, c] = 0; // Mark voxel as empty
                    c++;
                }
                c = 0;
                b++;
            }
            b = 0;
            a++;
        }

        //List<Voxel> voxelsToClear = new List<Voxel>();
        foreach(Voxel v in voxels){ //VOXEL IN REMAININGVOXELS //GET VECTOR3 GRID POSITION // SET VOXELDATA AT THAT POSITION TO 1
            if(v.GetVoxelObject() == null){
                voxelsToClear.Add(v);
                continue;
            }
            voxelData[v.GetGridPosition().x, v.GetGridPosition().y, v.GetGridPosition().z] = 1;
        }

        foreach(Voxel v in voxelsToClear){
            voxels.Remove(v);
        }*/
    }

    public void clearRemainingVoxels(){
        voxels.Clear();
    }

    [ContextMenu("Cut Up Large Grid")]

    public void CutUpLargeGrid(){
        int maxSmallGridSize = 16;
        int totalX = gridLocations.GetLength(0);
        int totalY = gridLocations.GetLength(1);
        int totalZ = gridLocations.GetLength(2);

        int numGridsX = Mathf.CeilToInt((float)totalX / maxSmallGridSize);
        int numGridsZ = Mathf.CeilToInt((float)totalZ / maxSmallGridSize);

        Debug.Log($"Num Grids X: {numGridsX}  Z: {numGridsZ}  Total: {totalX}x{totalY}x{totalZ}");

        arrayOfGridPieces = new GridPiece[numGridsX, numGridsZ];

        for(int x = 0; x < numGridsX; x++){
            for(int z = 0; z < numGridsZ; z++){
                // compute how many cells this chunk will actually contain (handles last chunk smaller than maxSize)
                int chunkSizeX = Mathf.Min(maxSmallGridSize, totalX - x * maxSmallGridSize);
                int chunkSizeZ = Mathf.Min(maxSmallGridSize, totalZ - z * maxSmallGridSize);

                int paddedSizeX = chunkSizeX + 1;
                int paddedSizeZ = chunkSizeZ + 1;

                byte[,,] smallData = new byte[paddedSizeX, totalY, paddedSizeZ];
                Vector3[,,] smallPositions = new Vector3[paddedSizeX, totalY, paddedSizeZ];

                for(int a = 0; a < paddedSizeX; a++)
                {
                    int sourceX = Mathf.Min(a + x * maxSmallGridSize, totalX - 1);

                    for(int b = 0; b < totalY; b++)
                    {
                        for(int c = 0; c < paddedSizeZ; c++)
                        {
                            int sourceZ = Mathf.Min(c + z * maxSmallGridSize, totalZ - 1);

                            smallData[a,b,c] = voxelData[sourceX,b,sourceZ];
                            smallPositions[a,b,c] = gridLocations[sourceX,b,sourceZ];
                        }
                    }
                }

                // compute edgeCases same as before (kept your existing mapping)
                int edgeCases = 0;
                if(x==0 && z==0) edgeCases = 1;
                else if(x==0 && !(z>=numGridsZ-1)) edgeCases = 2;
                else if(x==0 && z==numGridsZ-1) edgeCases = 3;
                else if(!(x>=numGridsX-1) && z==0) edgeCases = 4;
                else if(!(x>=numGridsX-1) && z==numGridsZ-1) edgeCases = 6;
                else if(x==numGridsX-1 && z==0) edgeCases = 7;
                else if(x==numGridsX-1 && !(z>=numGridsZ-1)) edgeCases = 8;
                else if(x==numGridsX-1 && z==numGridsZ-1) edgeCases = 9;
                else if(!(x>=numGridsX-1) && !(z>=numGridsZ-1)) edgeCases = 5;

                // IMPORTANT: pass the newly allocated arrays to GridPiece
                arrayOfGridPieces[x,z] = new GridPiece(smallData, smallPositions, reenableManager, x, z, edgeCases);
            }
        }

        // Now regenerate using the chunked grids (your GenerateVoxelGrid checks arrayOfGridPieces != null)
        GenerateVoxelGrid();
        parent.transform.GetChild(0).gameObject.SetActive(false);
    }


    /*public void CutUpLargeGrid(){
        int maxSmallGridSize = 32;
        int numGridsX = Mathf.CeilToInt((float)gridLocations.GetLength(0) / maxSmallGridSize);
        Debug.Log("Num Grids X: " + numGridsX);
        //int numGridsY = Mathf.CeilToInt((float)gridLocations.GetLength(1) / maxSmallGridSize);
        //Debug.Log("Num Grids Y: " + numGridsY);
        int numGridsZ = Mathf.CeilToInt((float)gridLocations.GetLength(2) / maxSmallGridSize);
        Debug.Log("Num Grids Z: " + numGridsZ);

        arrayOfGridPieces = new GridPiece[numGridsX, numGridsZ];
        byte[,,] smallData = new byte[maxSmallGridSize, gridLocations.GetLength(1), maxSmallGridSize];
        Vector3[,,] smallPositions = new Vector3[maxSmallGridSize, gridLocations.GetLength(1), maxSmallGridSize];
        

        for(int x=0; x<numGridsX; x++){
            for(int z=0; z<numGridsZ; z++){
                if(x<numGridsX-1 && z<numGridsZ-1){
                    Debug.Log("Went through 3rd part of if statment");
                    for(int a=0; a<maxSmallGridSize; a++){
                        for(int b=0; b<gridLocations.GetLength(1); b++){
                            for(int c=0; c<maxSmallGridSize; c++){
                                smallData[a,b,c] = voxelData[a+(maxSmallGridSize*x),b,c+(maxSmallGridSize*z)];
                                smallPositions[a,b,c] = gridLocations[a+(maxSmallGridSize*x),b,c+(maxSmallGridSize*z)];
                            } //This works for sections 1, 2, 4, & 5 in the 3x3 grid of possibilities
                        }
                    } 
                }else if(x==numGridsX-1 && z==numGridsZ-1){
                    Debug.Log("Went through 1st part of if statment");
                    for(int a=0; a<gridLocations.GetLength(0)-maxSmallGridSize; a++){
                        for(int b=0; b<gridLocations.GetLength(1); b++){
                            for(int c=0; c<gridLocations.GetLength(2)-maxSmallGridSize; c++){
                                Debug.Log("Calculating smallData at: " + a + ", " + b + ", " + c + " from large data at: " + (a+(maxSmallGridSize*x)) + ", " + b + ", " + (c+(maxSmallGridSize*z)));
                                smallData[a,b,c] = voxelData[a+((maxSmallGridSize-1)*x),b,c+((maxSmallGridSize-1)*z)];
                                smallPositions[a,b,c] = gridLocations[a+((maxSmallGridSize-1)*x),b,c+((maxSmallGridSize-1)*z)];
                            } //This works for section 9 in the 3x3 grid of possibilities
                        }
                    }
                }else if(x==numGridsX-1 || z==numGridsZ-1){
                    Debug.Log("Went through 2nd part of if statment");
                    for(int a=0; a<gridLocations.GetLength(0)-maxSmallGridSize; a++){
                        for(int b=0; b<gridLocations.GetLength(1); b++){
                            for(int c=0; c<gridLocations.GetLength(2)-maxSmallGridSize; c++){
                                Debug.Log("Calculating smallData at: " + a + ", " + b + ", " + c + " from large data at: " + (a+(maxSmallGridSize*x)) + ", " + b + ", " + (c+(maxSmallGridSize*z)));
                                smallData[a,b,c] = voxelData[a+(maxSmallGridSize*x),b,c+(maxSmallGridSize*z)];
                                smallPositions[a,b,c] = gridLocations[a+(maxSmallGridSize*x),b,c+(maxSmallGridSize*z)];
                            } //This should cover sections 3, 6, 7, & 8 in the 3x3 grid of possibilities
                        }
                    }
                }
                int edgeCases = 0;
                if(x==0 && z==0){
                    edgeCases = 1; //top left
                }
                else if(x==0 && !(z>=numGridsZ-1)){
                    edgeCases = 2; //top middle
                }
                else if(x==0 && z==numGridsZ-1){
                    edgeCases = 3; //top right
                }
                else if(!(x>=numGridsX-1) && z==0){
                    edgeCases = 4; //middle left
                }
                else if(!(x>=numGridsX-1) && z==numGridsZ-1){
                    edgeCases = 6; //middle right
                }
                else if(x==numGridsX-1 && z==0){
                    edgeCases = 7; //bottom left
                }
                else if(x==numGridsX-1 && !(z>=numGridsZ-1)){
                    edgeCases = 8; //bottom middle
                }
                else if(x==numGridsX-1 && z==numGridsZ-1){
                    edgeCases = 9; //bottom right
                }
                else if(!(x>=numGridsX-1) && !(z>=numGridsZ-1)){
                    edgeCases = 5; //middle middle
                }
                arrayOfGridPieces[x,z] = new GridPiece(smallData, smallPositions, reenableManager, x, z, edgeCases);
            }
        }
        //Impliment Cutting up of large grid in the same fashion that the large grid is made but with the new grid sizes.
        //Also figure out how to use the edgeCases variable to help with meshing... I just need proof of concept for now though.
        //So that Matt lets it move on and work even if at the moment with the edge Cases not working there will be weird seams.
        GenerateVoxelGrid();
    }*/

    [ContextMenu("Reset Small Grids")]

    public void resetSmallGrids(){
        arrayOfGridPieces = null;
    }

}

#endif