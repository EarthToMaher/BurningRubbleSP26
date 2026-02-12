using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Loading;
using TMPro;

public class AddCollidersToChildren : MonoBehaviour
{
    private int timeOfLastFrame;
    private List<GameObject> voxels = new List<GameObject>();
    [SerializeField] private float loadTime = 100f;
    private int numOfVoxels;
    private int numActivated = 0;
    [SerializeField] private int loadAmount = 100;
    [SerializeField] private Image loadingBar;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject loadingCam;
    private InputAction restart;
    private bool reloading = true;
    private bool loading = true;

    void Awake()
    {
        restart = InputSystem.actions.FindAction("Reset");
        foreach (Transform childTransform in this.transform)
        {
            //Debug.Log(childTransform);
            voxels.Add(childTransform.gameObject);
        }
        numOfVoxels = voxels.Count;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //FinalizeObjects();
        StartCoroutine(LoadObjects());
    }

    void FixedUpdate()
    {
        if (restart.WasPerformedThisFrame()&&!reloading) StartCoroutine(ReloadLevel());
    }

    public void AddBoxCollider(GameObject gameObject)
    {
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    public void AddDestructibleScript(GameObject gameObject)
    {
        gameObject.AddComponent<DestructibleBlock>();
    }

    public IEnumerator<WaitForEndOfFrame> LoadObjects()
    {
        int voxelCount = voxels.Count;
        do
        {
            for (int i = 0; i < loadAmount; i++)
            {
                AddBoxCollider(voxels[numActivated]);
                AddDestructibleScript(voxels[numActivated]);
                numActivated++;
                if (numActivated >= voxelCount) break;
            }
            loadingBar.fillAmount = ((float)numActivated / (float)voxelCount);
            yield return new WaitForEndOfFrame();
        } while (numActivated < voxelCount);
        Debug.Log("Finished Loading");
        SceneManager.LoadScene("Track1Ver1", LoadSceneMode.Additive);
        Destroy(loadingScreen);
        Destroy(loadingCam);
        reloading = false;
        Countdown[] kartActivate = FindObjectsByType<Countdown>(FindObjectsSortMode.None);
        loading = false;
        Countdown[] countdowns = FindObjectsByType<Countdown>(FindObjectsSortMode.None);
        foreach (Countdown countdown in countdowns)
        {
            StartCountdown(countdown);
            countdown.move.gameObject.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public void StartCountdown(Countdown countdown)
    {
        countdown.SetLevelLoaded(!loading);
    }
    
    public IEnumerator ReloadLevel()
    {
        reloading = true;
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync("Track1Ver1");
        yield return new WaitUntil(() => asyncUnload.isDone);
        //SceneManager.LoadScene("Track1Ver1", LoadSceneMode.Additive);
        reloading = false;
        SceneManager.LoadScene("LoadScene");


    }
}
