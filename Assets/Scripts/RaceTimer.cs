/*
Script for timing the race
Written By: Matthew Maher
Last Modified: 9/28/2025
*/

using UnityEngine;
using TMPro;
using System;
public class RaceTimer : MonoBehaviour
{
    [Tooltip("The text element we want to modify")]
    [SerializeField] private TextMeshProUGUI raceTime;
    [SerializeField] private TextMeshProUGUI bestTimeText;

    //Bool to check if the race is complete. Hidden in inspector as it shouldn't be modified from there
    [HideInInspector] public bool raceComplete = false;
    private float time = 0;

    private float bestTime = 0;

    private CheckpointDetection gm;

    private bool raceStarted = false;


    void Start()
    {

        //If we do not have a race timer, try to get one on this object
        if (raceTime == null) raceTime = GetComponent<TextMeshProUGUI>();
        gm = FindFirstObjectByType<CheckpointDetection>();
        LoadBestTime();
    }

    // Update is called once per frame
    void Update()
    {
        if(raceStarted)
        {
            time += Time.deltaTime;
        }

        //Don't update timer if the race is complete
        if (gm._lapCount > 3)
        {
            if (time < bestTime) SaveBestTime();
            return;
        }


        //time += Time.deltaTime;
        //Get the number of minutes, seconds, and milliseconds that the scene has been running
        int minutes = Mathf.FloorToInt(time) / 60;
        int seconds = Mathf.FloorToInt(time) % 60;
        int milliseconds = Mathf.FloorToInt((time - Mathf.FloorToInt(time)) * Mathf.Pow(10f, 3));

        //Format our time as a string and change the text
        String timeString = String.Format("{0}:{1}.{2}", minutes, seconds, milliseconds);
        raceTime.text = timeString;
    }

    public void LoadBestTime()
    {
        bestTime = PlayerPrefs.GetFloat("BestTime", 60000f);
        bestTimeText.text = "Best Time: " + FormatFloat(bestTime);
    }

    public void SaveBestTime()
    {
        PlayerPrefs.SetFloat("BestTime", time);
        PlayerPrefs.Save();
        LoadBestTime();
    }
    
    public String FormatFloat(float timeToFormat)
    {
        //Get the number of minutes, seconds, and milliseconds that the scene has been running
        int minutes = Mathf.FloorToInt(timeToFormat) / 60;
        int seconds = Mathf.FloorToInt(timeToFormat) % 60;
        int milliseconds = Mathf.FloorToInt((timeToFormat - Mathf.FloorToInt(timeToFormat)) * Mathf.Pow(10f, 3));

        //Format our time as a string and change the text
        return String.Format("{0}:{1}.{2}", minutes, seconds, milliseconds);
    }

    public void StartRace()
    {
        raceStarted = true;
    }
}
