/*
Script Representing Our Rubble Meter and it's Uses
Script Written By: Matthew Maher
Last Modified: 9/28/2025
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class RubbleMeter : MonoBehaviour
{
    //This is a change to attempt to put it in Richard's Branch
    //Current amount of rubble the  meter contains
    private int currRubbleAmt;
    [Tooltip("Integer representing the capacity of rubble a player can hold")]
    [SerializeField] private int MAX_AMT;

    [Tooltip("The image element that gets filled to represent the rubble meter")]
    [SerializeField] private Image rubbleBar;

    [Tooltip("Text displaying how many rubble charges we have")]
    [SerializeField] private TextMeshProUGUI rubbleText;

    [Tooltip("Amount needed to perform a rubble charge action")]
    [SerializeField] private int rubbleChargeAmt = 100;

    //Input action for using a rubble action
    private InputAction rubbleAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rubbleAction = GetComponent<PlayerInput>().actions["Rubble"];
    }

    // Update is called once per frame
    void Update()
    {
        //Temporary Debug to Perform a Rubble Action
        //if (rubbleAction.WasPerformedThisFrame()) UseRubble(rubbleChargeAmt); //UseRubble(rubbleChargeAmt);
    }

    /// <summary>
    /// Function for adding rubble to the rubble meter
    /// </summary>
    /// <param name="rubble">The amount of rubble added to the meter</param>
    public void GainRubble(int rubble)
    {
        //Adds the rubble and clamps the value between 0 and our max
        Debug.Log("Ran rubble gain");
        currRubbleAmt = Mathf.Clamp(currRubbleAmt + rubble, 0, MAX_AMT);
        UpdateUI();
    }

    public bool CanPerformRubbleAction()
    {
        if (currRubbleAmt >= rubbleChargeAmt) return true;
        return false;
    }


/// <summary>
/// Function for using rubble
/// </summary>
/// <param name="rubble">Amount of rubble being used in the action</param>
    public void UseRubble(int rubble)
    {
        //Check to make sure we have enough rubble to perform the action
        if (currRubbleAmt >= rubble)
        {
            currRubbleAmt -= rubble;
            UpdateUI();
        }
    }

    public void UseRubble()
    {
        UseRubble(rubbleChargeAmt);
    }


/// <summary>
/// Function for updating our UI
/// </summary>
    private void UpdateUI()
    {

        if (currRubbleAmt == MAX_AMT||rubbleChargeAmt==0)
        {
            rubbleBar.fillAmount = 1;
            rubbleText.text = "MAX";
        }
        else
        {
            int rubbleCharges = currRubbleAmt / (rubbleChargeAmt);
            rubbleText.text = "Rubble: " + rubbleCharges;
            float barFill = (currRubbleAmt - (rubbleCharges * rubbleChargeAmt)) / (float)rubbleChargeAmt;
            rubbleBar.fillAmount = barFill;
            Debug.Log(barFill);
        }
    }
}
