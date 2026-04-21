using UnityEngine;

public class MinimapIconRotationLock : MonoBehaviour
{
    public GameObject player;
    public float yOffset;
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.transform.position + new Vector3(0f,yOffset,0f);
        transform.rotation = Quaternion.Euler(90f,player.transform.eulerAngles.y,0f);
    }
}
