using UnityEngine;

public class GridPiece : MonoBehaviour
{
    private byte[,,] voxelData;
    private Vector3[,,] voxelPositions;
    private int sizeX, sizeY, sizeZ;
    private int maxSmallGridSize = 32;
    private int edgeCases = 0; //numbers 1-9 representing which edges are touching other grids via crafting table example
    private ReenableManager reenableManager;
    private int positionIn2DArrayGridX, positionIn2DArrayGridZ; //X is left to right, Z is top to bottom

    public GridPiece(){}

    public GridPiece(byte[,,] data, Vector3[,,] positions, ReenableManager rm, int positionIn2DArrayGridX, int positionIn2DArrayGridZ, int edgeCases)
    {
        voxelData = data;
        voxelPositions = positions;
        reenableManager = rm;
        this.positionIn2DArrayGridX = positionIn2DArrayGridX;
        this.positionIn2DArrayGridZ = positionIn2DArrayGridZ;
        this.edgeCases = edgeCases;
    }

    public byte[,,] GetVoxelData()
    {
        return voxelData;
    }

    public Vector3[,,] GetVoxelPositions()
    {
        return voxelPositions;
    }

    public int GetEdgeCases()
    {
        return edgeCases;
    }
    
}
