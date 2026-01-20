using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class ParryState : MonoBehaviour
{
    [SerializeField] private float parryWindow = 0.15f;
    [SerializeField] private GameObject parryRing;

    private InputSystem_Actions actions;
    
    void OnEnable()
    {
        if(actions == null) actions = new InputSystem_Actions();

        actions.Player.Parry.performed += ctx => TryParry();

        actions.Enable();
    }

    void OnDisable()
    {
        actions.Disable();
        
        actions.Player.Parry.performed -= ctx => TryParry();
    }

    public bool IsParrying { get; private set; }

    public void TryParry()
    {
        if (IsParrying) return;
        StartCoroutine(ParryRoutine());
    }

    private IEnumerator ParryRoutine()
    {
        IsParrying = true;
        parryRing.SetActive(true);
        
        yield return new WaitForSeconds(parryWindow);
        
        parryRing.SetActive(false);
        IsParrying = false;
    }

    public void Consume()
    {
        IsParrying = false;
    }
}