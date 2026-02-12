using System.Collections;
using UnityEngine;

public class PitStop : MonoBehaviour
{
    //This is a change to try to make it appear in Richard's branch
    [SerializeField] private int healRate;
    [SerializeField] private Transform exitDirection; //the direction the kart should boost out of the pit stop

    public IEnumerator HealObject(Kart kart)
    {
        if (!kart.NeedsHealing()) yield return new WaitForEndOfFrame();
        else
        {
            /* 
             * Old script for healing cart.
             */
            //kart.kartMovement.enabled = false;
            //kart.StopKart();
            //yield return new WaitForSeconds(1);
            //while (!kart.Heal(healRate)) yield return new WaitForSeconds(1);
            //kart.kartMovement.enabled = true;
            kart.Heal(healRate);
            kart.kartMovement.StartCoroutine(kart.kartMovement.Boost(60f, exitDirection.eulerAngles.y));
        }
    }
}
