using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;


[ExecuteInEditMode]
public class GridDrawer : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 1f;
    public Color gridColor = Color.gray;
    public bool includeInactive = false;
    public bool fitToRenderers = true;
    public bool fitToColliders = true;

    [Header("Layer Settings")]
    public string targetLayerName = "Voxelize";

    [Header("Cube Placement")]
    public GameObject cubePrefab;       // Optional prefab — if null, will use a built-in cube
    public bool clearOldCubes = true;   // Destroys previously placed cubes before spawning new ones
    public string spawnedParentName = "VoxelGrid";

    private Bounds sceneBounds;
    private int targetLayer;

    [ContextMenu("Generate Voxel Grid")]
    public void GenerateVoxelGrid()
    {
        targetLayer = LayerMask.NameToLayer(targetLayerName);
        if (targetLayer < 0)
        {
            Debug.LogWarning($"Layer \"{targetLayerName}\" not found. Please create it in the Tags & Layers settings.");
            return;
        }

        UpdateSceneBounds();

        // Optionally clear previously spawned cubes
        if (clearOldCubes)
        {
            var oldParent = GameObject.Find(spawnedParentName);
            if (oldParent != null)
                DestroyImmediate(oldParent);
        }

        // Create parent to hold cubes
        GameObject parent = new GameObject(spawnedParentName);
        parent.transform.position = Vector3.zero;

        Vector3 start = new Vector3(
            Mathf.Floor(sceneBounds.min.x / cellSize) * cellSize,
            Mathf.Floor(sceneBounds.min.y / cellSize) * cellSize,
            Mathf.Floor(sceneBounds.min.z / cellSize) * cellSize
        );

        Vector3 end = new Vector3(
            Mathf.Ceil(sceneBounds.max.x / cellSize) * cellSize,
            Mathf.Ceil(sceneBounds.max.y / cellSize) * cellSize,
            Mathf.Ceil(sceneBounds.max.z / cellSize) * cellSize
        );

        int count = 0;

        List<MeshCollider> mcList = GetMeshCollidersInLayer();


        // Main loop to fill the grid
        for (float x = start.x; x < end.x; x += cellSize)
        {
            for (float y = start.y; y < end.y; y += cellSize)
            {
                for (float z = start.z; z < end.z; z += cellSize)
                {

                    Vector3 cellCenter = new Vector3(x + cellSize / 2f, y + cellSize / 2f, z + cellSize / 2f);

                    if (IsPointInsideMesh(cellCenter, mcList))
                    {
                        GameObject cube;
                        if (cubePrefab != null)
                            cube = (GameObject)PrefabUtility.InstantiatePrefab(cubePrefab);
                        else
                            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        cube.transform.position = cellCenter;
                        cube.transform.localScale = Vector3.one * cellSize;
                        cube.transform.SetParent(parent.transform);

                        count++;
                    }
                }
            }
        }

        Debug.Log($"Generated {count} cubes inside grid fitting \"{targetLayerName}\" objects.");
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
        MeshCollider[] colliders = includeInactive ? FindObjectsOfType<MeshCollider>(true) : FindObjectsOfType<MeshCollider>();
        foreach (MeshCollider mc in colliders)
        {
            if (mc.gameObject.layer != targetLayer) continue;
            mcList.Add(mc);
        }
        Debug.Log("Found " + mcList.Count + " colliders in layer " + targetLayerName);
        return mcList;
    }

}
#endif