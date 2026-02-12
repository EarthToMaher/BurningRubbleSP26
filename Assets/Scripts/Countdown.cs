using UnityEngine;
using System.Collections;
using TMPro;

public class Countdown : MonoBehaviour
{
    [SerializeField] private float longBoost;
    [SerializeField] private float mediumBoost;
    [SerializeField] private float shortBoost;
    private bool gameStarted;
    private bool isActive;
    private float intensity;
    private float count;
    private bool hasBoosted;
    private bool isCounting;
    private bool levelLoaded = false;
    [SerializeField] public KartMovement move;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private RaceTimer raceTimer;
    private AddCollidersToChildren levelStarter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelStarter = FindAnyObjectByType<AddCollidersToChildren>();
        levelStarter.StartCountdown(this);
        isActive = true;
        hasBoosted = false;
        isCounting = false;
        gameStarted = false;
        count = 3;

        //-1 value used to determine whether intensity has been set already
        intensity = -1;
    }

    private void Update()
    {
        Debug.Log("Level Loaded: " + levelLoaded);
        if(gameStarted&&levelLoaded)
        {
            Debug.Log("Work pls");
            if (count > 0)
            {
                count -= Time.deltaTime;
            }

            if (move.GetAccelerateValue() != 0)
            {
                if (count > 2) { intensity = 0; }
                else if (count >= 1.7f && intensity == -1) { intensity = longBoost; }
                else if (count >= 1.4f && intensity == -1) { intensity = mediumBoost; }
                else if (count > 1 && intensity == -1) { intensity = shortBoost; }
                else if (count > 0 && intensity == -1) { intensity = 0; }
            }
            else if (count > 0 && intensity != -1) { intensity = -1; }

            if (count <= 0)
            {
                isActive = false;
                raceTimer.StartRace();
                if (!hasBoosted)
                {
                    if (intensity > 0)
                    {
                        Debug.Log("Calling coroutine...");
                        move.StartCoroutine(move.StartBoost(intensity));
                        Debug.Log("Boost with intensity: " + intensity);
                    }
                    hasBoosted = true;
                }
            }

            if (count.ToString("F0").Equals("0"))
            {
                countText.SetText("GO!");
                StartCoroutine(RemoveCountdown());
            }
            else { countText.SetText(count.ToString("F0")); }


            //DEBUG: show countdown with 1 decimal place
            //Debug.Log("Count: " + count.ToString("F1"));
        }
    }

    public bool GetActive()
    {
        return isActive;
    }

    public void SetGameStarted(bool hasStarted)
    {
        gameStarted = hasStarted;
        //Debug.Log("Countdown triggered");
    }

    public void SetLevelLoaded(bool val)
    {
        this.levelLoaded =val;
        Debug.Log("Setting level loaded");
    }

    private IEnumerator RemoveCountdown()
    {
        yield return new WaitForSeconds(0.7f);
        this.gameObject.SetActive(false);
    }
    
    private IEnumerator WaitToStart()
    {
        //waiting for kart to be on the ground (only relevant in MVP scene)
        //call coroutine from Start and wrap all code in Update in an if(isCounting) statement
        yield return new WaitForSeconds(3f);
        isCounting = true;
    }
}
