#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class VoxelGridBaker : MonoBehaviour
{
    [Header("Voxel Settings")]
    public GameObject voxelRoot; // Parent of your voxel cubes/meshes
    public string prefabPath = "Assets/BakedVoxelGrid.prefab";
    public string meshPath = "Assets/BakedVoxelMesh.asset";

    [Header("Options")]
    public bool mergeMeshes = true;       // Merge all cubes into one mesh
    public bool deleteOriginal = true;    // Delete runtime cubes after baking

    [ContextMenu("Bake Voxel Grid Persistently")]
    public void BakeVoxelGrid()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("You must enter Play Mode to bake the voxel grid!");
            return;
        }

        if (voxelRoot == null)
        {
            Debug.LogError("Voxel Root GameObject is not assigned!");
            return;
        }

        GameObject bakedGO;

        if (mergeMeshes)
        {
            bakedGO = MergeChildMeshes(voxelRoot);
        }
        else
        {
            bakedGO = Instantiate(voxelRoot);
            bakedGO.name = "BakedVoxelGrid";
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(bakedGO, prefabPath);
        Debug.Log($"Voxel grid baked and saved as prefab at {prefabPath}");

        // Optional cleanup
        if (deleteOriginal)
        {
            Destroy(voxelRoot);
            Debug.Log("Original runtime voxel objects deleted.");
        }
    }

    private GameObject MergeChildMeshes(GameObject parent)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
        {
            Debug.LogError("No meshes found under voxel root!");
            return null;
        }

        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh == null) continue;
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh mergedMesh = new Mesh();
        mergedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // For large voxel grids
        mergedMesh.CombineMeshes(combine);

        // Save mesh as a permanent asset
        AssetDatabase.CreateAsset(mergedMesh, meshPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Merged mesh saved to {meshPath}");

        // Create new GameObject with merged mesh
        GameObject mergedGO = new GameObject("BakedVoxelGrid");
        MeshFilter mf = mergedGO.AddComponent<MeshFilter>();
        mf.sharedMesh = mergedMesh;
        MeshRenderer mr = mergedGO.AddComponent<MeshRenderer>();

        // Assign first material found
        MeshRenderer firstRenderer = parent.GetComponentInChildren<MeshRenderer>();
        if (firstRenderer != null)
        {
            mr.sharedMaterial = firstRenderer.sharedMaterial;
        }

        return mergedGO;
    }
}
#endif