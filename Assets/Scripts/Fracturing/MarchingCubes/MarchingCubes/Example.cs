using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using ProceduralNoiseProject;
using Common.Unity.Drawing;

namespace MarchingCubesProject
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    [ExecuteInEditMode]
    public enum MARCHING_MODE { CUBES, TETRAHEDRON };

    public class Example : MonoBehaviour
    {

        public Material material;

        public MARCHING_MODE mode = MARCHING_MODE.CUBES;

        public int seed = 0;

        public bool smoothNormals = false;

        public bool drawNormals = false;

        private List<GameObject> meshes = new List<GameObject>();

        private NormalRenderer normalRenderer;

        //New Variables, goes from World Voxelization
        public WorldVoxelization worldVoxelization;

        /*void Start()
        {

            INoise perlin = new PerlinNoise(seed, 1.0f);
            FractalNoise fractal = new FractalNoise(perlin, 3, 1.0f);

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            Marching marching = null;
            if (mode == MARCHING_MODE.TETRAHEDRON)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            //The size of voxel array.
            int width;
            int height;
            int depth;
            if (worldVoxelization.gridLocations != null)
            {
                width = worldVoxelization.gridLocations.GetLength(0);
                height = worldVoxelization.gridLocations.GetLength(1);
                depth = worldVoxelization.gridLocations.GetLength(2);
            } else {
                width = 32;
                height = 32;
                depth = 32;
            }

            var voxels = new VoxelArray(width, height, depth);

            //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = x / (width - 1.0f);
                        float v = y / (height - 1.0f);
                        float w = z / (depth - 1.0f);

                        voxels[x, y, z] = fractal.Sample3D(u, v, w);
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            //Create the normals from the voxel.

            if (smoothNormals)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = verts[i];

                    float u = p.x / (width - 1.0f);
                    float v = p.y / (height - 1.0f);
                    float w = p.z / (depth - 1.0f);

                    Vector3 n = voxels.GetNormal(u, v, w);

                    normals.Add(n);
                }

                normalRenderer = new NormalRenderer();
                normalRenderer.DefaultColor = Color.red;
                normalRenderer.Length = 0.25f;
                normalRenderer.Load(verts, normals);
            }

            var position = new Vector3(-width / 2, -height / 2, -depth / 2);

            CreateMesh32(verts, normals, indices, position);

        }*/

        private void CreateMesh32(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position, GridPiece gridPiece)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = worldVoxelization.parent.transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.convex = true; //Importaint for proper collision detection with rigidbodies
            collider.isTrigger = true;
            DestructibleMesh dm = go.AddComponent<DestructibleMesh>();
            dm.idX = gridPiece.positionIn2DArrayGridX;
            dm.idZ = gridPiece.positionIn2DArrayGridZ;
            Debug.Log("Setting dm parent grid piece to: " + worldVoxelization.arrayOfGridPieces[dm.idX, dm.idZ]);
            dm.gridPiece = worldVoxelization.arrayOfGridPieces[dm.idX,dm.idZ]; //Need to set this properly later //need to bring this back to WorldVOxelization
            dm.voxelData = gridPiece.GetVoxelData();
            dm.voxelPositions = gridPiece.GetVoxelPositions();
            go.transform.localPosition = position;

            meshes.Add(go);
        }

        private void CreateMesh32Big(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position, byte[,,] voxelData, Vector3[,,] gridLocations)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = worldVoxelization.parent.transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            MeshCollider collider = go.AddComponent<MeshCollider>();
            collider.convex = true; //Importaint for proper collision detection with rigidbodies
            collider.isTrigger = true;
            DestructibleMesh dm = go.AddComponent<DestructibleMesh>();
            //dm.parentGridPiece = gridPiece; //Need to set this properly later
            dm.voxelData = voxelData;
            dm.voxelPositions = gridLocations;
            go.transform.localPosition = position;

            meshes.Add(go);
        }

        /// <summary>
        /// UPDATE - Unity now supports 32 bit indices so the method is optional.
        /// 
        /// A mesh in unity can only be made up of 65000 verts.
        /// Need to split the verts between multiple meshes.
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="position"></param>
        /*private void CreateMesh16(List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {

            int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
            int numMeshes = verts.Count / maxVertsPerMesh + 1;

            for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<Vector3> splitNormals = new List<Vector3>();
                List<int> splitIndices = new List<int>();

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

                    if (idx < verts.Count)
                    {
                        splitVerts.Add(verts[idx]);
                        splitIndices.Add(j);

                        if (normals.Count != 0)
                            splitNormals.Add(normals[idx]);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Mesh mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt16;
                mesh.SetVertices(splitVerts);
                mesh.SetTriangles(splitIndices, 0);

                if (splitNormals.Count > 0)
                    mesh.SetNormals(splitNormals);
                else
                    mesh.RecalculateNormals();

                mesh.RecalculateBounds();

                GameObject go = new GameObject("Mesh");
                go.transform.parent = transform;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = material;
                go.GetComponent<MeshFilter>().mesh = mesh;
                go.transform.localPosition = position;

                meshes.Add(go);
            }
        }*/

        /*private void Update()
        {
            //transform.Rotate(Vector3.up, 10.0f * Time.deltaTime);
        }*/

        private void OnRenderObject()
        {
            if (normalRenderer != null && meshes.Count > 0 && drawNormals)
            {
                var m = meshes[0].transform.localToWorldMatrix;

                normalRenderer.LocalToWorld = m;
                normalRenderer.Draw();
            }

        }

        [ContextMenu("Generate Marching Cubes Mesh")]
        public void GenerateMarchingCubesMesh(GridPiece gridPiece)
        {
            byte[,,] voxelData = gridPiece.GetVoxelData();
            Vector3[,,] gridLocations = gridPiece.GetVoxelPositions();
            //INoise perlin = new PerlinNoise(seed, 1.0f);
            //FractalNoise fractal = new FractalNoise(perlin, 3, 1.0f);

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            Marching marching = null;
            if (mode == MARCHING_MODE.TETRAHEDRON)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            //The size of voxel array.
            int width;
            int height;
            int depth;
            if (gridLocations != null &&gridLocations.Length > 0)
            {
                width = gridLocations.GetLength(0);
                height = gridLocations.GetLength(1);
                depth = gridLocations.GetLength(2);
            }
            else
            {
                width = 32;
                height = 32;
                depth = 32;
            }

            var voxels = new VoxelArray(width, height, depth); //Creates a 3D array of floats called voxels.


            //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = x / (width - 1.0f);
                        float v = y / (height - 1.0f);
                        float w = z / (depth - 1.0f);

                        //voxels[x, y, z] = fractal.Sample3D(u, v, w); //fills the voxel array with a float number from the fractal noise, typically between -1 and 1. 
                        // but there is a weird -4 in there

                        //Experimental: try to use the voxel data from world voxelization
                        voxels[x, y, z] = voxelData[x, y, z];

                        //Debug.Log("Sampling voxel at: " + x + ", " + y + ", " + z + "With value of: " + fractal.Sample3D(u, v, w) + "\nTo double check: " +voxels[x, y, z]);
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>(); //List to hold the vertices of the mesh but only holds 1 for each cube
            List<Vector3> normals = new List<Vector3>(); //List to hold the normals of the mesh, only needs one for each vertex (I think)
            List<int> indices = new List<int>(); //List to hold the indices of the mesh, not sure what this is for.

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            //Create the normals from the voxel.

            if (smoothNormals)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = verts[i];

                    float u = p.x / (width - 1.0f);
                    float v = p.y / (height - 1.0f);
                    float w = p.z / (depth - 1.0f);

                    Vector3 n = voxels.GetNormal(u, v, w);

                    normals.Add(n);
                }

                normalRenderer = new NormalRenderer();
                normalRenderer.DefaultColor = Color.red;
                normalRenderer.Length = 0.25f;
                normalRenderer.Load(verts, normals);
            }

            var position = new Vector3(gridLocations[0, 0, 0].x, gridLocations[0, 0, 0].y, gridLocations[0, 0, 0].z);
            //position = transform.TransformPoint(position);

            Debug.Log("Creating Mesh at position: " + position);

            CreateMesh32(verts, normals, indices, position, gridPiece);

        }

        //I hate having to do this twice but I cant figure out how to pass in the correct data otherwise
        public void GenerateMarchingCubesMeshBig(byte[,,] voxelData, Vector3[,,] gridLocations)
        {
            //INoise perlin = new PerlinNoise(seed, 1.0f);
            //FractalNoise fractal = new FractalNoise(perlin, 3, 1.0f);

            //Set the mode used to create the mesh.
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
            Marching marching = null;
            if (mode == MARCHING_MODE.TETRAHEDRON)
                marching = new MarchingTertrahedron();
            else
                marching = new MarchingCubes();

            //Surface is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
            //The target value does not have to be the mid point it can be any value with in the range.
            marching.Surface = 0.0f;

            //The size of voxel array.
            int width;
            int height;
            int depth;
            if (gridLocations != null &&gridLocations.Length > 0)
            {
                width = gridLocations.GetLength(0);
                height = gridLocations.GetLength(1);
                depth = gridLocations.GetLength(2);
            }
            else
            {
                width = 32;
                height = 32;
                depth = 32;
            }

            var voxels = new VoxelArray(width, height, depth); //Creates a 3D array of floats called voxels.


            //Fill voxels with values. Im using perlin noise but any method to create voxels will work.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        float u = x / (width - 1.0f);
                        float v = y / (height - 1.0f);
                        float w = z / (depth - 1.0f);

                        //voxels[x, y, z] = fractal.Sample3D(u, v, w); //fills the voxel array with a float number from the fractal noise, typically between -1 and 1. 
                        // but there is a weird -4 in there

                        //Experimental: try to use the voxel data from world voxelization
                        voxels[x, y, z] = voxelData[x, y, z];

                        //Debug.Log("Sampling voxel at: " + x + ", " + y + ", " + z + "With value of: " + fractal.Sample3D(u, v, w) + "\nTo double check: " +voxels[x, y, z]);
                    }
                }
            }

            List<Vector3> verts = new List<Vector3>(); //List to hold the vertices of the mesh but only holds 1 for each cube
            List<Vector3> normals = new List<Vector3>(); //List to hold the normals of the mesh, only needs one for each vertex (I think)
            List<int> indices = new List<int>(); //List to hold the indices of the mesh, not sure what this is for.

            //The mesh produced is not optimal. There is one vert for each index.
            //Would need to weld vertices for better quality mesh.
            marching.Generate(voxels.Voxels, verts, indices);

            //Create the normals from the voxel.

            if (smoothNormals)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = verts[i];

                    float u = p.x / (width - 1.0f);
                    float v = p.y / (height - 1.0f);
                    float w = p.z / (depth - 1.0f);

                    Vector3 n = voxels.GetNormal(u, v, w);

                    normals.Add(n);
                }

                normalRenderer = new NormalRenderer();
                normalRenderer.DefaultColor = Color.red;
                normalRenderer.Length = 0.25f;
                normalRenderer.Load(verts, normals);
            }

            var position = new Vector3(gridLocations[0, 0, 0].x, gridLocations[0, 0, 0].y, gridLocations[0, 0, 0].z);
            //position = transform.TransformPoint(position);

            Debug.Log("Creating Mesh at position: " + position);

            CreateMesh32Big(verts, normals, indices, position, voxelData, gridLocations);

        }

        [ContextMenu("Regenerate Marching Cubes Mesh from new Voxel Data")]
        public void RegenerateMarchingCubesMesh(byte[,,] newByteArray, Vector3[,,] gridLocations)
        {
            //Delete old meshes
            foreach (var mesh in meshes)
            {
                Debug.Log("Deleting old mesh: " + mesh.name);
                Destroy(mesh);
            }
            meshes.Clear();

            //worldVoxelization.recheckVoxels();

            //Generate new mesh
            //newByteArray; //Need to update this function to accept the new structure of data I am going with
            GenerateMarchingCubesMeshBig(newByteArray, gridLocations);
        }

        public void RegenerateMarchingCubesMeshSmall(DestructibleMesh dm, GridPiece gridPiece)
        {
            //Delete old meshes
            
            
            //meshes.Clear();
            Destroy(dm.gameObject);

            meshes.Remove(dm.gameObject);
            //worldVoxelization.recheckVoxels();

            //Generate new mesh
            //newByteArray; //Need to update this function to accept the new structure of data I am going with
            GenerateMarchingCubesMesh(gridPiece);
        }

        public void ReenableMeshRegeneration()
        {
            
        }
    }

}
