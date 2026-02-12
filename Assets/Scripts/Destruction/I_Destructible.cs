using UnityEngine;

public interface I_Destructible
{
    /// <summary>
    /// Implementable function for "destroying" this object
    /// </summary>
    /// <param name="instigator">The entity that caused this to occur (i.e., players, world).</param>
    /// <param name="cause">The physical thing causing the destruction (i.e., kart, item)</param>
    public void DestroyMe(GameObject instigator, GameObject cause);
}