using UnityEngine;

public class CollisionWithWalls : MonoBehaviour
{
    private ScreenShake mainCamera;
    private ScreenShake UICamera;


    private void OnCollisionEnter(Collision other) {
        mainCamera = other.transform.parent.GetChild(2).GetComponent<ScreenShake>(); //2
        UICamera = other.transform.parent.GetChild(3).GetComponent<ScreenShake>(); //3
        if(mainCamera != null && UICamera != null)
        {
            if (other.gameObject.tag == "Player")
            {
                mainCamera.run();
            }
        }
    }
}
