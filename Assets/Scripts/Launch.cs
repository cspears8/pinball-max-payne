using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// THIS IS ALL AWFUL PROTOTYPE CODE BUT JUST TESTING - CS
public class Launch : MonoBehaviour
{
    [SerializeField] private float power = 20f;
    [SerializeField] private float powerPerSecond = 5f;
    [SerializeField] private Image chargeImage;
    private bool canLaunch = false;
    private bool charging = false;
    private InputSystem_Actions actions;
    private float currentPower = 0f;
    private BallControl player;

    private void Awake()
    {
        player = FindFirstObjectByType<BallControl>();
    }

    private void OnEnable()
    {
        if(actions == null) actions = new InputSystem_Actions();

        actions.Player.Launch.started += StartCharge;
        actions.Player.Launch.canceled += ActuallyLaunch;
        
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
        
        actions.Player.Launch.started -= StartCharge;
        actions.Player.Launch.canceled -= ActuallyLaunch;
    }

    private void ActuallyLaunch(InputAction.CallbackContext context)
    {
        charging = false;

        if (currentPower > 0 && canLaunch)
        {
            player.LaunchSelf(currentPower);
        }

        currentPower = 0f;
        chargeImage.DOKill();
        chargeImage.fillAmount = 0f;
    }

    private void StartCharge(InputAction.CallbackContext context)
    {
        if (canLaunch)
        {
            charging = true;
            StartCoroutine(ChargeLaunch());
        }
        else
        {
            Debug.Log("Can't start charge");
        }
    }

    private IEnumerator ChargeLaunch()
    {
        while (charging && currentPower < power)
        {
            currentPower += powerPerSecond * Time.deltaTime;
            currentPower = Mathf.Clamp(currentPower, 0, power);

            chargeImage.DOFillAmount(currentPower/power, 0.1f);

            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ball")) canLaunch = true;
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ball")) canLaunch = false;
    }
}
