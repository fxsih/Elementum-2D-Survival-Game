using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.5f;

    [Header("Dash FX")]
    public GameObject dashFxPrefab;

    [Header("Jump (Visual Only)")]
    public float jumpHeight = 0.55f;
    public float jumpDuration = 0.22f;
    public AnimationCurve jumpCurve;

    [Header("Shadow")]
    public Vector2 shadowMinScale = new Vector2(0.25f, 0.15f);
    public float shadowLeftOffsetX = -0.01f;

    [Header("Run Dust FX")]
    public GameObject runDustPrefab;
    public Vector2 runDustOffsetRight = Vector2.zero;
    public Vector2 runDustOffsetLeft = new Vector2(-0.5f, 0f);

    Rigidbody2D rb;
    SpriteRenderer sprite;
    Animator animator;

    Transform visual;
    Transform shadow;

    GameObject activeRunDust;
    bool wasRunning;

    PlayerInputActions input;

    Vector2 moveInput;
    Vector2 mouseScreenPos;
    Vector2 dashDirection;

    bool isJumping;
    bool isDashing;
    bool dashLocked;

    float dashLockTimer;
    float jumpTimer;
    float dashTimer;

    Vector3 visualBasePos;
    Vector3 shadowBasePos;
    Vector3 shadowBaseScale;

    public bool IsJumping => isJumping;
    public bool IsDashing => isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponentInChildren<Animator>();
        sprite = animator.GetComponent<SpriteRenderer>();

        visual = animator.transform.parent;
        shadow = transform.Find("Shadow");

        visualBasePos = visual.localPosition;
        shadowBasePos = shadow.localPosition;
        shadowBaseScale = shadow.localScale;
    }

    void OnEnable()
    {
        input = new PlayerInputActions();
        input.Gameplay.Enable();

        input.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Gameplay.Move.canceled += _ => moveInput = Vector2.zero;

        input.Gameplay.MousePosition.performed += ctx => mouseScreenPos = ctx.ReadValue<Vector2>();
        input.Gameplay.Jump.performed += _ => StartJump();
        input.Gameplay.Dash.performed += _ => TryStartDash();
    }

    void OnDisable()
    {
        input.Gameplay.Disable();
    }

    void FixedUpdate()
    {
        if (isDashing)
            rb.linearVelocity = dashDirection * dashSpeed;
        else
            rb.linearVelocity = moveInput * moveSpeed;
    }

    void Update()
    {
        UpdateFacing();
        UpdateAnimation();
        UpdateJump();
        UpdateShadowOffset();
        UpdateRunDust();
        UpdateDash();
        UpdateDashCooldown();
    }

    // ===================== DASH =====================

    void TryStartDash()
    {
        if (isDashing) return;
        if (dashLocked) return;

        StartDash();
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;

        if (moveInput.sqrMagnitude > 0.01f)
            dashDirection = moveInput.normalized;
        else
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
            );
            dashDirection = (mouseWorld - transform.position).normalized;
        }

        visual.gameObject.SetActive(false);
        DestroyRunDust();
        SpawnDashFX();
    }

    void UpdateDash()
    {
        if (!isDashing) return;

        dashTimer += Time.deltaTime;

        if (dashTimer >= dashDuration)
        {
            isDashing = false;
            rb.linearVelocity = Vector2.zero;
            visual.gameObject.SetActive(true);

            dashLocked = true;
            dashLockTimer = 0f;
        }
    }

    void UpdateDashCooldown()
    {
        if (!dashLocked) return;

        dashLockTimer += Time.deltaTime;

        if (dashLockTimer >= dashCooldown)
            dashLocked = false;
    }

    void SpawnDashFX()
    {
        if (!dashFxPrefab) return;

        float angle =
            Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg + 180f;

        GameObject fx = Instantiate(
            dashFxPrefab,
            transform.position,
            Quaternion.Euler(0f, 0f, angle),
            transform
        );

        Destroy(fx, dashDuration);
    }

    // ===================== OTHER SYSTEMS =====================

    void UpdateFacing()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
        );

        sprite.flipX = mouseWorld.x < transform.position.x;
    }

    void UpdateAnimation()
    {
        animator.SetFloat("Speed", rb.linearVelocity.sqrMagnitude);
    }

    void StartJump()
    {
        if (isJumping) return;
        if (isDashing) return;   // block jump during dash

        isJumping = true;
        jumpTimer = 0f;
        animator.SetTrigger("Jump");
    }

    void UpdateJump()
    {
        if (!isJumping)
        {
            visual.localPosition = visualBasePos;
            shadow.localScale = shadowBaseScale;
            return;
        }

        jumpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);
        float h = jumpCurve.Evaluate(t);

        visual.localPosition = visualBasePos + Vector3.up * (h * jumpHeight);
        shadow.localScale = Vector3.Lerp(shadowBaseScale, shadowMinScale, h);

        if (t >= 1f)
        {
            isJumping = false;
            visual.localPosition = visualBasePos;
            shadow.localScale = shadowBaseScale;
        }
    }

    void UpdateShadowOffset()
    {
        shadow.localPosition = shadowBasePos;

        if (sprite.flipX)
        {
            shadow.localPosition = new Vector3(
                shadowBasePos.x + shadowLeftOffsetX,
                shadowBasePos.y,
                shadowBasePos.z
            );
        }
    }

    // ===================== FIXED RUN DUST =====================

    void UpdateRunDust()
    {
        // 🔥 Use REAL velocity instead of input
        bool isActuallyMoving =
            rb.linearVelocity.sqrMagnitude > 0.01f &&
            !isJumping &&
            !isDashing;

        if (isActuallyMoving && !wasRunning)
            SpawnRunDust();

        if (!isActuallyMoving && wasRunning)
            DestroyRunDust();

        wasRunning = isActuallyMoving;

        if (!activeRunDust) return;

        Vector2 offset = sprite.flipX ? runDustOffsetLeft : runDustOffsetRight;
        activeRunDust.transform.localPosition = offset;

        SpriteRenderer sr = activeRunDust.GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.flipX = sprite.flipX;
    }

    void SpawnRunDust()
    {
        if (!runDustPrefab || activeRunDust) return;
        activeRunDust = Instantiate(runDustPrefab, transform);
    }

    void DestroyRunDust()
    {
        if (!activeRunDust) return;
        Destroy(activeRunDust);
        activeRunDust = null;
    }
}