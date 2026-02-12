using UnityEngine;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using ProceduralNoiseProject;
using Common.Unity.Drawing;

/// <summary>
/// Generates a 3x3x3 voxel cube mesh at runtime, complete with voxel data.
/// </summary>
public class VoxelCubeMesh : MonoBehaviour
{
    public int size = 32;
    public float voxelSize = 1.0f;

    public Vector3[,,] voxelPositions;
    public byte[,,] voxelData;

    public Material material;

    private Mesh mesh;

    void Start()
    {
        GenerateVoxelData();
        GenerateMesh();
    }

    void GenerateVoxelData()
    {
        voxelPositions = new Vector3[size, size, size];
        voxelData = new byte[size, size, size];

        Vector3 origin = Vector3.zero;

        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        for (int z = 0; z < size; z++)
        {
            Vector3 pos = origin + new Vector3(x, y, z) * voxelSize;
            voxelPositions[x, y, z] = pos;
            voxelData[x, y, z] = 1; // all solid initially
        }
    }

    void GenerateMesh()
    {
        // Simple cube made up of 3x3x3 smaller cubes merged together
        // Each cube contributes faces that are not adjacent to another solid voxel
        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();

        // Offsets for cube faces
        Vector3[] faceNormals =
        {
            Vector3.forward, Vector3.back,
            Vector3.up, Vector3.down,
            Vector3.right, Vector3.left
        };

        // Each face has 4 vertices (a quad)
        Vector3[,] faceVertices = new Vector3[6, 4]
        {
            // Forward
            {
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f)
            },
            // Back
            {
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f)
            },
            // Up
            {
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f)
            },
            // Down
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f)
            },
            // Right
            {
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f)
            },
            // Left
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f)
            }
        };

        Vector3 origin = new Vector3(size, size, size) * 0.5f * voxelSize;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    if (voxelData[x, y, z] == 0) continue;

                    Vector3 pos = new Vector3(x, y, z) * voxelSize - origin;
                    voxelPositions[x, y, z] = pos;
                    voxelData[x, y, z] = 1; // all solid initially

                    // For each face, check if adjacent voxel exists
                    for (int face = 0; face < 6; face++)
                    {
                        Vector3Int neighbor = new Vector3Int(x, y, z) + Vector3Int.RoundToInt(faceNormals[face]);
                        bool neighborSolid = false;

                        if (neighbor.x >= 0 && neighbor.x < size &&
                            neighbor.y >= 0 && neighbor.y < size &&
                            neighbor.z >= 0 && neighbor.z < size)
                        {
                            neighborSolid = voxelData[neighbor.x, neighbor.y, neighbor.z] == 1;
                        }

                        if (neighborSolid) continue; // don’t draw internal faces

                        int startIndex = vertices.Count;
                        for (int i = 0; i < 4; i++)
                            vertices.Add(pos + faceVertices[face, i] * voxelSize);

                        triangles.Add(startIndex + 0);
                        triangles.Add(startIndex + 1);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 2);
                        triangles.Add(startIndex + 3);
                        triangles.Add(startIndex + 0);
                    }
                }

        // Create the mesh
        CreateMesh32(vertices, faceNormals, triangles);
        //mesh.SetVertices(vertices);
        //mesh.SetTriangles(triangles, 0);
        //mesh.RecalculateNormals();

        // Assign to components
        var mf = GetComponent<MeshFilter>();
        //mf.sharedMesh = mesh;

        var mr = GetComponent<MeshRenderer>();
        if (mr.sharedMaterial == null)
            mr.sharedMaterial = new Material(Shader.Find("Standard"));
    }
    
        private void CreateMesh32(List<Vector3> verts, Vector3[] normals, List<int> indices)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            //if (normals.Length > 0)
                //mesh.SetNormals(normals);
            //else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            GameObject go = this.gameObject;
            //go.transform.parent = worldVoxelization.parent.transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<MeshRenderer>().material = material;
            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.convex = true; //Importaint for proper collision detection with rigidbodies
            collider.isTrigger = true; //So that it doesn't interfere with physics but can still detect collisions
            DestructibleMesh dm = go.AddComponent<DestructibleMesh>(); //Add DestructibleMesh script to handle destruction
            dm.voxelData = voxelData;
            dm.voxelPositions = voxelPositions;
        }
}