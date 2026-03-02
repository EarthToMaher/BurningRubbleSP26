using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
/*
This class handles the non-movement related behavior of the kart such as:
Gaining Rubble
Losing HP
Respawning
*/
public class Kart : MonoBehaviour, I_Damageable
{
    [System.Serializable]
    public class HP
    {
        [SerializeField] private int hp;
        [SerializeField] private Image hpImage;
        [SerializeField] private TextMeshProUGUI hpText;
        private int MAX_HP;
        public void ModifyHP(int Amt)
        {
            hp = Mathf.Clamp(hp+Amt,0,MAX_HP);
        }
        public bool IsDead()
        {
            return hp <= 0;
        }

        public bool AtMax()
        {
            return hp ==  MAX_HP;
        }
        public void UpdateHPUI()
        {
            if(hpText != null) hpText.text = "Health: " + hp;
            if(hpImage!= null)hpImage.fillAmount = hp* 1.0f / MAX_HP * 1.0f;
        }

        public void ResetHP()
        {
            hp = MAX_HP;
        }

        public void SetMaxHP()
        {
            MAX_HP = hp;
        }
    }
    [SerializeField] private HP hPSettings;

    [System.Serializable]

    public class Rubble
    {
        private int currRubbleAmt;
        [Tooltip("Integer representing the capacity of rubble a player can hold")]
        [SerializeField] private int MAX_AMT;

        [Tooltip("The image element that gets filled to represent the rubble meter")]
        [SerializeField] private Image rubbleBar;

        [Tooltip("Text displaying how many rubble charges we have")]
        [SerializeField] private TextMeshProUGUI rubbleText;

        [Tooltip("Amount needed to perform a rubble charge action")]
        [SerializeField] private int rubbleChargeAmt = 100;
        [Tooltip("Amount needed to perform a rubble charge action")]
        [SerializeField] private float rubbleAngle = 100;
        [Tooltip("Amount needed to perform a rubble charge action")]
        [SerializeField] private float rubbleBoostLength = 100;

        public void GainRubble(int amt)
        {
            currRubbleAmt = Mathf.Clamp(currRubbleAmt+amt, 0, MAX_AMT);
            UpdateUI();
        }
        public float GetRubbleBoostLength()
        {
            return rubbleBoostLength;
        }

        public float GetRubbleAngle()
        {
            return rubbleAngle;
        }

        public bool CanPerformRubbleAction()
        {
            return currRubbleAmt >= rubbleChargeAmt;
        }

        public void UseRubble()
        {
            UseRubble(rubbleChargeAmt);
        }

        public void UseRubble(int amt)
        {
            currRubbleAmt-=amt;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (rubbleBar == null || rubbleText == null) Debug.LogWarning("Rubble: " + currRubbleAmt);
            if (currRubbleAmt == MAX_AMT||rubbleChargeAmt==0)
            {
                rubbleBar.fillAmount = 1;
                rubbleText.text = "MAX";
            }
        else
        {
            int rubbleCharges = currRubbleAmt / rubbleChargeAmt;
            rubbleText.text = "Rubble: " + rubbleCharges;
            float barFill = (currRubbleAmt - (rubbleCharges * rubbleChargeAmt)) / (float)rubbleChargeAmt;
            rubbleBar.fillAmount = barFill;
        }
        }
    }

    [SerializeField] public Rubble rubbleSettings;
    
    public class Respawn
    {
        
    }

    [SerializeField] private float iFrameLength;
    private bool invincible = false;
    private CarControl kartControls;

    private InputAction rubbleAction;
    private Rigidbody rb;

    void Awake()
    {
        if (gameObject.GetComponent<CarControl>() != null) kartControls = gameObject.GetComponent<CarControl>();
        else if (gameObject.GetComponentInChildren<CarControl>() != null) kartControls = gameObject.GetComponentInChildren<CarControl>();
        else Debug.LogError("NO CAR CONTROL FOUND");

        hPSettings.SetMaxHP();

        rb = GetComponent<Rigidbody>();


        rubbleAction = GetComponent<PlayerInput>().actions["Rubble"];
        //restart = InputSystem.actions.FindAction("Reset");
    }
    void Update()
    {
        //transform.position = new Vector3(transform.position.x, 2.001f, transform.position.z);
        //if (rubbleAction.WasPerformedThisFrame()) RubbleBoost();
        //if (restart.WasPerformedThisFrame()) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void TakeDamage(int dmg)
    {
        if (invincible) return;
        hPSettings.ModifyHP(-dmg);
        UpdateUI();
        if (hPSettings.IsDead()) KartDeath();
        else StartCoroutine(IFrames());
    }

    public IEnumerator IFrames()
    {
        invincible = true;
        yield return new WaitForSeconds(iFrameLength);
        invincible = false;
    }

    public bool Heal(int healAmt)
    {
        hPSettings.ModifyHP(healAmt);
        UpdateUI();
        return hPSettings.AtMax();
    }

    private void KartDeath()
    {
        invincible = true;
        //GameObject _lapManager = GameObject.Find("LapManager");
        kartControls.StopKart();
        kartControls.SetReceivingInput(false);
        StartCoroutine(HealUponDeath());
            CheckpointDetection _checkDetect = FindFirstObjectByType<CheckpointDetection>();
            Vector3 _respawnPoint = FindFirstObjectByType<LapManager>().SetCheckpointPos(_checkDetect._currCheckpoint);
            Quaternion _respawnRotation = FindAnyObjectByType<LapManager>().SetCheckpointRot(_checkDetect._currCheckpoint);
            this.transform.position = _respawnPoint;
            this.transform.rotation = _respawnRotation;
    }

    private IEnumerator HealUponDeath()
    {
        yield return new WaitForFixedUpdate();
        hPSettings.ResetHP();
        invincible = false;
        UpdateUI();
        kartControls.SetReceivingInput(true);
    }

    public bool NeedsHealing()
    {
        return !hPSettings.AtMax();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (hPSettings.IsDead()) return;
        PitStop pitStop = collision.gameObject.GetComponent<PitStop>();
        if (pitStop != null) StartCoroutine(pitStop.HealObject(this));
    }

    public void UpdateUI()
    {
        hPSettings.UpdateHPUI();
    }

    public void LateUpdate()
    {
        if (hPSettings.IsDead())
        {
            Debug.Log("Kart death");
            KartDeath();
        }
    }

    public void RubbleBoost(float hInput, float vInput)
    {
        /*
        if (rubbleMeter.CanPerformRubbleAction() && kartMovement.CanMove())
        {
            StartCoroutine(BecomeInvincible(2f));
            StartCoroutine(kartMovement.RubbleBoost(rubbleBoostIntensity));
            rubbleMeter.UseRubble();
        }*/

        Vector2 boostDirection = new Vector2(hInput, vInput); //Get our input direction
        if (boostDirection == Vector2.zero) boostDirection = new Vector2(0, 1f); //If 0, just go forward
        else{
        //Vector3 localDirection = new Vector3(boostDirection.x, 0f, boostDirection.y); //Covert to a vector 3
        //Vector3 worldDirection = transform.TransformDirection(localDirection).normalized; //Do it in world space
        //Vector3 clampedDirection = Vector3.RotateTowards(transform.forward, worldDirection, Mathf.Deg2Rad * rubbleSettings.GetRubbleAngle(), 0f).normalized; //Clamp it to our angle
        //rb.MoveRotation(Quaternion.Euler(clampedDirection));
        }
        kartControls.RubbleBoost(rubbleSettings.GetRubbleBoostLength());


    }

    public IEnumerator BecomeInvincible(float seconds)
    {
        if (invincible) StopCoroutine("BecomeInvincible");
        invincible = true;
        yield return new WaitForSeconds(seconds);
        invincible = false;
    }
}
