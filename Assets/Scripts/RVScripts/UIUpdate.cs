using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

public class UIUpdate : MonoBehaviour
{
    // Didn't add a Awake/Start call for these because tbh I don't want write a script to search up and down parents/children
    [SerializeField] private Kart _kartScript;
    [SerializeField] private Image _healthUI;
    [SerializeField] private Image _rubbleUI;

    public void Update()
    {
        // Update UI within Update
        UpdateHealthUI();
        UpdateRubbleUI();
    }

    public void UpdateHealthUI()
    {
        float sliderValue = Mathf.Clamp((float)_kartScript.GetCurrHP() / _kartScript.GetCurrMaxHP(), 0f, 1f);
        //Debug.Log("Current hp: " + _kartScript.GetCurrHP() / _kartScript.GetCurrMaxHP());
        _healthUI.fillAmount = sliderValue;
        
    }

    public void UpdateRubbleUI()
    {
        float sliderValue = Mathf.Clamp((float)_kartScript.GetCurrentRubble() / _kartScript.GetCurrMaxRubble(), 0f, 1f);
        _rubbleUI.fillAmount = sliderValue;
    }
}
