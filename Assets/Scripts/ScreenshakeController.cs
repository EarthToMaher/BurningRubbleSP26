using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshakeController : MonoBehaviour
{
    [SerializeField] private AnimatorController controller;
    private Animator screenShakeAnim;
    private InputAction ss;

    void Start()
    {
        screenShakeAnim = gameObject.AddComponent<Animator>();
        screenShakeAnim.runtimeAnimatorController = controller;
        ss = InputSystem.actions.FindAction("ScreenshakeDebug");
    }

    void Update()
    {
        if(ss.WasPressedThisFrame())
        {
            Debug.Log("shake requested");
            Shake();
        }
    }

    public void Shake()
    {
        screenShakeAnim.Play("ScreenShake");
    }
}
