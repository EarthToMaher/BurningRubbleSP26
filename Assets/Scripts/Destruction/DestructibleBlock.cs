using UnityEngine;
/*
Written By: Matthew Maher
Last Updated: 9/21/2025
*/

public class DestructibleBlock : MonoBehaviour, I_Destructible
{

    [Header("Rubble")]
    [Tooltip("How much rubble the instigator gains")]
    [SerializeField] private int rubble = 5;
    [Tooltip("Prefab for the rubble chunk that spawns off of this")]
    [SerializeField] private GameObject rubblePickUp;
    [Tooltip("Number of rubble pick ups that spawn")]
    [SerializeField] private int numOfPickUps;


    [Tooltip("Represents the amount of damage taken when crashing into this block")]
    [SerializeField] private int hp = 5;

    [Tooltip("Prefab for our particle effect")]
    [SerializeField] private GameObject particle;

    //Private Instance Variables, No Need to Access from Inspector
    private MeshRenderer rend;
    private Collider coll;
    private ReenableManager reMgr;
    private RubblePickUp[] pickUps;

    // Initializes our instance variables, Instantiates our PickUps and Fills the Array
    void Start()
    {

        rend = gameObject.GetComponent<MeshRenderer>();
        coll = gameObject.GetComponent<Collider>();
        reMgr = FindFirstObjectByType<ReenableManager>();
        Debug.Log("Assigned reMgr");
        if(particle!=null) particle = Instantiate(particle, transform.position, Quaternion.identity);


        if (numOfPickUps < 1) return;
        pickUps = new RubblePickUp[numOfPickUps];
        for (int i = 0; i < numOfPickUps; i++)
        {
            GameObject rubbleObj = Instantiate(rubblePickUp, transform.position, Quaternion.identity);
            pickUps[i] = rubbleObj.GetComponent<RubblePickUp>();
        }
    }

    /// <summary>
    /// Function for "destroying" this object
    /// </summary>
    /// <param name="instigator">The entity that caused this to occur (i.e., players, world).</param>
    /// <param name="cause">The physical thing causing the destruction (i.e., kart, item)</param>
    public void DestroyMe(GameObject instigator, GameObject cause)
    {
        I_Damageable damageable = cause.GetComponent<I_Damageable>();
        if (damageable != null) damageable.TakeDamage(hp);
        RubbleMeter rm = instigator.GetComponent<RubbleMeter>();
        if (rm != null) rm.GainRubble(rubble);
        if(particle!=null)particle.SetActive(true);
        SetObjectActive(false);
        reMgr.AddToBatch(this);
        Vector2 launchDirection = new Vector2(1, 1);
        Rigidbody causeRb = cause.GetComponent<Rigidbody>();
        if (causeRb != null) launchDirection = new Vector2(causeRb.linearVelocity.x, causeRb.linearVelocity.z);
        if (numOfPickUps > 0) foreach (RubblePickUp pickUp in pickUps) pickUp.Spawn(launchDirection);
    }

    /// <summary>
    /// Function for reenabling destroyed blocks
    /// </summary>
    public void RepairMe()
    {
        //TODO: Repair Animation
        if (numOfPickUps > 0) foreach (RubblePickUp pickUp in pickUps) pickUp.Despawn();
        SetObjectActive(true);
    }

    /// <summary>
    /// Controls disabling/reenabling destructible objects
    /// </summary>
    /// <param name="state">Object enabled state.</param>
    public void SetObjectActive(bool state)
    {
        rend.enabled = state;
        coll.enabled = state;
    }

    //Debug Function, Can be Called in Inspector
    [ContextMenu("DestroyMe")]
    public void DestroyTest()
    {
        DestroyMe(this.gameObject, this.gameObject);
    }
}
