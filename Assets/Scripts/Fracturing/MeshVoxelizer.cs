using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class MeshVoxelizer : MonoBehaviour
{
    [SerializeField] private Vector3 voxelSize = new Vector3(0.5f, 0.5f, 0.5f);

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshCollider mc = GetComponent<MeshCollider>();

        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("No mesh found on object to voxelize.");
            return;
        }

        // Local-space bounds of the mesh
        Bounds bounds = mf.sharedMesh.bounds;

        // Iterate through bounding box in local space
        for (float x = bounds.min.x; x < bounds.max.x; x += voxelSize.x)
        {
            for (float y = bounds.min.y; y < bounds.max.y; y += voxelSize.y)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += voxelSize.z)
                {
                    // Center of this voxel in local space
                    Vector3 localPos = new Vector3(
                        x + voxelSize.x * 0.5f,
                        y + voxelSize.y * 0.5f,
                        z + voxelSize.z * 0.5f
                    );

                    // Convert to world space
                    Vector3 worldPos = transform.TransformPoint(localPos);

                    // Only spawn voxel if inside
                    if (IsPointInsideMesh(worldPos, mc))
                    {
                        GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        voxel.transform.localScale = voxelSize;
                        voxel.transform.position = worldPos;
                        voxel.transform.SetParent(transform, true);
                    }
                }
            }
        }
    }

    bool IsPointInsideMesh(Vector3 point, MeshCollider mc)
    {
        // Cast a ray upward and count intersections
        Ray ray = new Ray(point, Vector3.up);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        int count = 0;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == mc)
                count++;
        }

        // Odd = inside, even = outside
        return (count % 2) == 1;
    }
}
