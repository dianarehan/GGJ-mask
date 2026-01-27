using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MaskCarController : MonoBehaviour
{
    [Header("--- Normal Car Settings ---")]
    [Tooltip("How fast the car accelerates.")]
    public float acceleration = 10f;
    [Tooltip("Maximum speed in normal mode.")]
    public float maxSpeed = 15f;
    [Tooltip("How fast the car turns.")]
    public float turnSpeed = 150f;
    [Tooltip("0 = Ice (Slide), 1 = Sticky (Go Kart). Recommended: 0.9")]
    [Range(0, 1)] public float driftFactor = 0.9f;
    [Tooltip("Drag when not accelerating (Natural slow down).")]
    public float drag = 3f;

    [Header("--- Mask Dash Settings ---")]
    [Tooltip("The explosive speed when Space is pressed.")]
    public float dashForce = 50f;
    [Tooltip("How long the uncontrolled dash lasts.")]
    public float dashDuration = 3f;
    [Tooltip("Drag during dash (0 means you don't slow down at all).")]
    public float dashDrag = 0f;

    [Header("--- Physics Materials ---")]
    [Tooltip("Assign a material with Friction: 0.4, Bounciness: 0")]
    public PhysicsMaterial2D normalMat;
    [Tooltip("Assign a material with Friction: 0, Bounciness: 1")]
    public PhysicsMaterial2D bouncyMat;

    // Internal State
    private Rigidbody2D rb;
    private Collider2D col;
    private float moveInput;
    private float turnInput;
    private bool isDashing = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0; // Ensure no gravity
    }

    void Update()
    {
        // 1. INPUT HANDLING
        if (!isDashing)
        {
            moveInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
            turnInput = Input.GetAxisRaw("Horizontal"); // A = -1, D = 1

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(DashRoutine());
            }
        }
        else
        {
            // Lock inputs during dash
            moveInput = 0;
            turnInput = 0;
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // In dash mode, we just let physics take the wheel.
            // But we ensure the car faces the direction it is flying (optional, looks better)
            if (rb.linearVelocity.sqrMagnitude > 1f)
            {
                float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = angle;
            }
            return;
        }

        // --- NORMAL CAR PHYSICS ---

        // 2. Kill Orthogonal Velocity (The "Car" feel)
        // This removes sideways sliding. If driftFactor is 1, it kills ALL sliding.
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + rightVelocity * (1f - driftFactor); // Apply drift correction

        // 3. Acceleration
        if (moveInput > 0)
        {
            // Only add force if under max speed
            if (rb.linearVelocity.magnitude < maxSpeed)
            {
                rb.AddRelativeForce(Vector2.up * acceleration, ForceMode2D.Force);
            }
            rb.linearDamping = 0; // Remove drag while gas is pressed
        }
        else
        {
            rb.linearDamping = drag; // Apply drag when coasting
        }

        // 4. Steering
        // Only allow steering if the car is actually moving (Realism)
        float minSpeedToTurn = 0.5f;
        if (rb.linearVelocity.magnitude > minSpeedToTurn)
        {
            rb.rotation -= turnInput * turnSpeed * Time.fixedDeltaTime;
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;

        // ENTER DASH: Swap to Bouncy Physics
        col.sharedMaterial = bouncyMat;
        col.enabled = false; col.enabled = true; // Refresh collider

        rb.linearDamping = dashDrag;

        // Launch Force
        rb.AddRelativeForce(Vector2.up * dashForce, ForceMode2D.Impulse);

        // TODO: Enable Mask UI Here

        yield return new WaitForSeconds(dashDuration);

        // EXIT DASH: Swap to Normal Physics
        col.sharedMaterial = normalMat;
        col.enabled = false; col.enabled = true; // Refresh collider

        rb.linearDamping = drag;

        // TODO: Disable Mask UI Here

        isDashing = false;
    }
}