using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    [Header("Time Scale")]
    [SerializeField] private float slowScale = 0.2f;
    [SerializeField] private float enterSpeed = 10f;
    [SerializeField] private float exitSpeed = 10f;

    [Header("Resource")] 
    [SerializeField] private float maxEnergy = 3f;
    [SerializeField] private float drainRate = 1f;
    [SerializeField] private float rechargeRate = 0.75f;
    [SerializeField] private Image fillImage;
    
    [Header("Juice")]
    [SerializeField] private MMF_Player enterFeedback;

    private float energy;
    private bool slowing;
    private InputSystem_Actions actions;

    void Awake()
    {
        energy = maxEnergy;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f; //Cap delta time manually to this rate
    }

    void OnEnable()
    {
        actions ??= new InputSystem_Actions();
        
        actions.Player.TimeSlow.started += _ => StartSlowing();
        actions.Player.TimeSlow.canceled += _ => StopSlowing();

        actions.Enable();
    }

    void OnDisable()
    {
        actions.Disable();
        
        actions.Player.TimeSlow.started -= _ => StartSlowing();
        actions.Player.TimeSlow.canceled -= _ => StopSlowing();
    }

    void Update()
    {
        HandleEnergy();
        HandleTimeScale();
    }

    void StartSlowing()
    {
        slowing = true;
        enterFeedback?.PlayFeedbacks();
    }

    void StopSlowing()
    {
        slowing = false;
        enterFeedback?.StopFeedbacks();
    }

    void HandleEnergy()
    {
        if(slowing && energy > 0f)
            energy -= drainRate * Time.unscaledDeltaTime;
        else
            energy += rechargeRate * Time.unscaledDeltaTime; //Should probably replace this with an actual way to reward movement

        energy = Mathf.Clamp(energy, 0f, maxEnergy);

        fillImage.fillAmount = NormalizedEnergy;

        if (energy <= 0f)
            StopSlowing();
    }

    void HandleTimeScale()
    {
        float target = (slowing && energy > 0f) ? slowScale : 1f;
        float speed = slowing ? enterSpeed : exitSpeed;
        
        Time.timeScale = Mathf.Lerp(
            Time.timeScale, 
            target, 
            Time.deltaTime * speed
            );
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void ResetTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public float NormalizedEnergy => energy / maxEnergy;
}
