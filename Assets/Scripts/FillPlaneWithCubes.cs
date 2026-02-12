///THIS IS ALL GPT, AND IS ONLY MeANT AS A QUICK TeST TOOL FOR DeSTRUCTION

using UnityEngine;

public class FillPlaneWithCubes : MonoBehaviour
{
    [Tooltip("The cube prefab to instantiate (must be 1x1x1 in size)")]
    public GameObject cubePrefab;

    [Tooltip("Optional parent for cubes (to keep hierarchy clean)")]
    public Transform cubesParent;

    void Start()
    {
        if (cubePrefab == null)
        {
            Debug.LogError("Cube Prefab is not assigned!");
            return;
        }

        // Get the size of the plane based on its scale
        Vector3 planeScale = transform.localScale;

        // Unity's default plane is 10x10 units at scale 1
        int width = Mathf.RoundToInt(planeScale.x * 10);
        int height = Mathf.RoundToInt(planeScale.z * 10);

        // Get the plane's position
        Vector3 planePos = transform.position;

        // Loop to fill the plane with cubes
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 cubePosition = new Vector3(
                    planePos.x - width / 2f + x + 0.5f,
                    planePos.y + 0.5f,
                    planePos.z - height / 2f + z + 0.5f
                );

                GameObject cube = Instantiate(cubePrefab, cubePosition, Quaternion.identity);

                if (cubesParent != null)
                {
                    cube.transform.parent = cubesParent;
                }
            }
        }
    }
}
