using UnityEngine;

public class MultiplayerMapActivator : MonoBehaviour
{
    [SerializeField] private MPManager _mpManager;
    [SerializeField] private JoinScreenManager _jsManager;
    [SerializeField] private GameObject _singlePlayerMap;
    [SerializeField] private GameObject _multiPlayerMap;

    private void Update()
    {
        _jsManager = FindFirstObjectByType<JoinScreenManager>();
        if (_mpManager.GetPlayerNum() > 1 && _jsManager == null)
        {
            _multiPlayerMap.SetActive(true);
        } else if (_mpManager.GetPlayerNum() == 1 && _jsManager == null)
        {
            _singlePlayerMap.SetActive(true);
        }
    }

}
