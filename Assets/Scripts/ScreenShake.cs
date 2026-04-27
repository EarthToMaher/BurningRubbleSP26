using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    //public bool start = false;
    public AnimationCurve curve;
    public float duration = 1f;
    void Update()
    {
        
    }

    public void run()
    {
        StartCoroutine(Shaking());
    }

    IEnumerator Shaking()
    {
        Vector3 startingPosition = transform.position;
        float elapsedTime = 0f;

        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration);
            transform.position += Random.insideUnitSphere * strength;
            yield return null;
        }
    }
}
