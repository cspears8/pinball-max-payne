using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BallControl : MonoBehaviour
{
    [SerializeField] private float maxTetherDistance = 10f;
    [SerializeField] private LayerMask tetherMask;
    [SerializeField] private Transform arrowTransform;

    private LineRenderer line;
    private DistanceJoint2D joint;
    private Rigidbody2D rb;
    private InputSystem_Actions actions;
    private Vector2 aimVector;
    private Vector2 moveVector;
    private Camera cam;
    private bool tetherActive = false;
    private Vector2 tetherAnchorWorld;
    private Rigidbody2D tetherBody;
    private Vector2 tetherAnchorLocal;
    private SpriteRenderer arrowSprite;
    private SpriteRenderer sr;

    void Awake()
    {
        cam = Camera.main;
        
        rb = GetComponent<Rigidbody2D>();
        if(rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.angularDamping = 0.05f;
        rb.linearDamping = 0.1f;
            
        line = GetComponent<LineRenderer>();
        if(line == null) line = gameObject.AddComponent<LineRenderer>();
        
        line.positionCount = 2;
        line.enabled = false;
        
        joint = GetComponent<DistanceJoint2D>();
        if (joint == null) joint = gameObject.AddComponent<DistanceJoint2D>();
        
        joint.enabled = false;
        joint.autoConfigureDistance = false;
        joint.maxDistanceOnly = true;
        
        arrowSprite = arrowTransform.GetComponentInChildren<SpriteRenderer>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        if(actions == null) actions = new InputSystem_Actions();
        
        actions.Player.Tether.performed += ctx => TryTether();
        actions.Player.Tether.canceled += ctx => ReleaseTether();
        
        actions.Player.Look.performed += ctx => aimVector = ctx.ReadValue<Vector2>();
        actions.Player.Look.canceled += ctx => aimVector = Vector2.zero;

        actions.Player.Move.performed += ctx => moveVector = ctx.ReadValue<Vector2>();
        actions.Player.Move.canceled += ctx => moveVector = Vector2.zero;
        
        actions.Player.Retry.performed += ctx => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        actions.Enable();
    }

    void OnDisable()
    {
        actions.Disable();
        
        actions.Player.Tether.performed -= ctx => TryTether();
        actions.Player.Tether.canceled -= ctx => ReleaseTether();
        
        actions.Player.Look.performed -= ctx => aimVector = Vector2.zero;
        actions.Player.Look.canceled -= ctx => aimVector = Vector2.zero;
        
        actions.Player.Retry.performed -= ctx => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        UpdateArrow();
        UpdateTether();
        UpdateDirection();
    }

    private void UpdateDirection()
    {
        Vector3 dir = rb.linearVelocity;
        if(dir.y < -0.1f || dir.y > 0.1f)
        {
            dir += new Vector3(moveVector.x, moveVector.y, 0) * (Time.deltaTime * 10f);
            rb.linearVelocity = dir;
        }
    }

    private void UpdateTether()
    {
        if (tetherActive)
        {
            Vector2 end = tetherBody != null
                ? tetherBody.transform.TransformPoint(tetherAnchorLocal)
                : tetherAnchorWorld;
            
            line.SetPosition(0, rb.position);
            line.SetPosition(1, end);
        }
    }

    private void UpdateArrow()
    {
        Vector2 aimDir = GetAimDirection();
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
        arrowTransform.localRotation = Quaternion.Euler(0, 0, angle);
        if (ValidTetherAim()) arrowSprite.color = Color.blue;
        else arrowSprite.color = Color.white;
    }

    public void TryTether()
    {
        RaycastHit2D hit = ValidTetherAim();
        if (!hit) return;
        
        tetherActive = true;
        
        joint.enabled = true;
        joint.autoConfigureDistance = false;
        joint.maxDistanceOnly = true;
        
        joint.connectedBody = hit.collider.attachedRigidbody;

        //For rotating rope, check if we have a rigidbody, if so use local space for end, otherwise use world
        tetherBody = hit.rigidbody;
        if(tetherBody!= null) 
            tetherAnchorLocal = tetherBody.transform.InverseTransformPoint(hit.point);
        else 
            tetherAnchorWorld = hit.point;
        
        if (hit.rigidbody == null) joint.connectedAnchor = hit.point;
        
        
        joint.distance = Vector2.Distance(rb.position, hit.point);
        
        UpdateAimLine(hit.point);
    }

    RaycastHit2D ValidTetherAim()
    {
        Vector2 aimDirection = GetAimDirection();
        
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            aimDirection.normalized,
            maxTetherDistance,
            tetherMask
        );

        return hit;
    }

    private Vector2 GetAimDirection()
    {
        if (aimVector.sqrMagnitude > 0.1f) return aimVector.normalized;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0;

        Vector2 aimDirection = (mouseWorld - (Vector3)rb.position);
        if (aimDirection.sqrMagnitude < 0.001f) return rb.linearVelocity.normalized;

        return aimDirection.normalized;

    }

    public Vector2 GetParryAim()
    {
        //Return aimed direction
        if(aimVector.sqrMagnitude > 0.1f) return aimVector.normalized;
        
        //Return direction of our velocity
        if(rb.linearVelocity.sqrMagnitude > 0.01f) return rb.linearVelocity.normalized;
        
        //Fall back
        return Vector2.zero;
    }

    public void UpdateAimLine(Vector2 hit)
    {
        line.enabled = true;
        line.SetPosition(0, rb.position);
        line.SetPosition(1, hit);
    }

    public void HideLine()
    {
        line.enabled = false;
    }

    public void ReleaseTether()
    {
        tetherActive = false;
        joint.enabled = false;
        joint.connectedAnchor = Vector2.zero;
        joint.connectedBody = null;
        HideLine();
    }
    
    public void LaunchSelf(float currentPower, Vector3 dir)
    {
        rb.AddForce(dir * currentPower, ForceMode2D.Impulse);
    }

    public void Hold()
    {
        rb.bodyType = RigidbodyType2D.Static;
        sr.enabled = false;
    }

    public void Release()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        sr.enabled = true;
    }
}