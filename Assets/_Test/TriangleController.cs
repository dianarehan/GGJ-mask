using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))] // Added Collider requirement
public class ShipMovement : MonoBehaviour
{
    public enum ShipState
    {
        Normal,
        Dash
    }

    [Header("State Info")]
    public ShipState currentState = ShipState.Normal;

    [Header("Engine Settings (Normal)")]
    public float acceleration = 20f;
    public float maxSpeed = 10f;
    public float turnSpeed = 200f;

    [Header("Handling Settings (Normal)")]
    public float deceleration = 10f;
    [Range(0, 1)] public float driftFactor = 0.9f;

    [Header("Dash Settings (Chaos)")]
    public float dashForce = 40f;      // Instant explosion of speed
    public float dashDuration = 3f;    // How long you lose control
    public PhysicsMaterial2D normalMat; // Assign your Friction 0.4 mat
    public PhysicsMaterial2D bouncyMat; // Assign your Friction 0, Bounce 1 mat

    private Rigidbody2D rb;
    private Collider2D col;
    private float turnInput;
    private bool isGasPressed;
    private bool isDashPressed;
    private float dashTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 2f;
    }

    void Update()
    {
        turnInput = Input.GetAxisRaw("Horizontal");
        isGasPressed = Input.GetKey(KeyCode.W);
        isDashPressed = Input.GetKeyDown(KeyCode.Space);

        // Trigger Dash
        if (isDashPressed && currentState == ShipState.Normal)
        {
            SwitchState(ShipState.Dash);
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case ShipState.Normal:
                HandleNormalState();
                break;

            case ShipState.Dash:
                HandleDashState();
                break;
        }
    }

    // --- STATE LOGIC ---

    void HandleNormalState()
    {
        // 1. STEERING
        if (turnInput != 0)
        {
            float forwardEffect = rb.linearVelocity.magnitude / maxSpeed;
            forwardEffect = Mathf.Clamp(forwardEffect, 0.3f, 1f);
            float newAngle = rb.rotation - (turnInput * turnSpeed * forwardEffect * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);
        }

        // 2. DRIFT CORRECTION
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        rb.linearVelocity = forwardVelocity + (rightVelocity * (1f - driftFactor));

        // 3. ACCELERATE
        if (isGasPressed)
        {
            rb.AddRelativeForce(Vector2.up * acceleration);
        }
        else
        {
            // 4. BRAKE
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // 5. SPEED LIMITER
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void HandleDashState()
    {
        // 1. COUNTDOWN
        dashTimer -= Time.fixedDeltaTime;

        // 2. CHECK EXIT
        if (dashTimer <= 0)
        {
            SwitchState(ShipState.Normal);
            return;
        }

        // 3. FORCE ROTATION TO FACE VELOCITY (The Fix)
        // If we are moving, turn the ship to face the movement direction
        if (rb.linearVelocity.sqrMagnitude > 1f)
        {
            // Calculate the angle of the velocity vector
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;

            // Force the rotation. This overrides any collision spin.
            rb.MoveRotation(angle);
        }
    }

    public void SwitchState(ShipState newState)
    {
        currentState = newState;

        if (newState == ShipState.Dash)
        {
            // --- ENTER CHAOS (DASH) ---
            dashTimer = dashDuration;

            // 1. SWAP MATERIAL (The Fix)
            // We swap it on the Rigidbody to ensure it overrides everything
            rb.sharedMaterial = bouncyMat;
            col.sharedMaterial = bouncyMat; // Swap collider too just in case

            // 2. FORCE REFRESH
            // Toggling the collider forces the physics engine to recalculate friction immediately
            col.enabled = false;
            col.enabled = true;

            // 3. PHYSICS SETTINGS
            rb.linearDamping = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 4. LAUNCH
            rb.AddRelativeForce(Vector2.up * dashForce, ForceMode2D.Impulse);
        }
        else if (newState == ShipState.Normal)
        {
            // --- ENTER CONTROL (NORMAL) ---

            // 1. SWAP MATERIAL (The Fix)
            rb.sharedMaterial = normalMat;
            col.sharedMaterial = normalMat;

            // 2. FORCE REFRESH
            col.enabled = false;
            col.enabled = true;

            // 3. RESTORE SETTINGS
            rb.linearDamping = 0f; // Manual braking handles this
            rb.angularDamping = 2f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.angularVelocity = 0f;
        }
    }
}