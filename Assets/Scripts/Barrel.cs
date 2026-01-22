using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [SerializeField] private float power = 20f;
    [SerializeField] private float rotationSpeed = 10f;

    private bool canLaunch = false;
    private BallControl player;
    private InputSystem_Actions actions;
    private float rotation = 0f;
    private Rigidbody2D rb => GetComponent<Rigidbody2D>();

    private void Awake()
    {
        player = FindFirstObjectByType<BallControl>();
    }

    private void OnEnable()
    {
        if(actions == null) actions = new InputSystem_Actions();

        actions.Player.Launch.performed += OnLaunch;
        
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Player.Launch.performed -= OnLaunch;

        actions.Disable();
    }

    private void OnLaunch(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!canLaunch) return;

        canLaunch = false;
        player.Release();

        Vector2 dir = Quaternion.Euler(0, 0, rotation) * Vector2.up;
        player.LaunchSelf(power, dir);
        
        StartCoroutine(IgnoreTrigger());
    }

    private bool ignoreTrigger = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ignoreTrigger) return;
        if (other.gameObject == player.gameObject)
        {
            canLaunch = true;
            player.Hold();
        }
    }

    private IEnumerator IgnoreTrigger()
    {
        ignoreTrigger = true;
        yield return new WaitForSeconds(0.2f);
        ignoreTrigger = false;
    }

    // This is awful rotation, but blahhh - CS
    void FixedUpdate()
    {
        rotation += rotationSpeed * Time.fixedDeltaTime;
        rotation %= 360f;
        rb.SetRotation(rotation);
    }
}
