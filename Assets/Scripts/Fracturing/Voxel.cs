using UnityEngine;

public class Voxel : MonoBehaviour
{
    private Vector3Int gridPosition;
    private GameObject voxelObject;
    private byte voxelValue;
    public Voxel(Vector3Int gridPos, GameObject obj)
    {
        gridPosition = gridPos;
        voxelObject = obj;
    }
    public Voxel(Vector3Int gridPos, byte val)
    {
        gridPosition = gridPos;
        voxelObject = null;
        voxelValue = val;
    }
    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }
    public GameObject GetVoxelObject()
    {
        return voxelObject;
    }
    public void SetVoxelObject(GameObject obj)
    {
        voxelObject = obj;
    }
    public void SetGridPosition(Vector3Int pos)
    {
        gridPosition = pos;
    }
    public byte GetVoxelValue()
    {
        return voxelValue;
    }
    public void SetVoxelValue(byte val)
    {
        voxelValue = val;
    }
}
