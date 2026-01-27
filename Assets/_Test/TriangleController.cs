using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipMovement : MonoBehaviour
{
    [Header("Engine Settings")]
    public float acceleration = 20f;
    public float maxSpeed = 10f;
    public float turnSpeed = 200f;

    [Header("Handling Settings")]
    [Tooltip("How fast you stop when releasing W.")]
    public float deceleration = 10f;
    [Tooltip("1 = Sharp Turns (Car). 0 = Space Drift. Recommended: 0.9")]
    [Range(0, 1)] public float driftFactor = 0.9f;

    private Rigidbody2D rb;
    private float turnInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 2f;
    }

    void Update()
    {
        turnInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        // 1. STEERING
        if (turnInput != 0)
        {
            float forwardEffect = rb.linearVelocity.magnitude / maxSpeed;
            forwardEffect = Mathf.Clamp(forwardEffect, 0.5f, 1f);
            float newAngle = rb.rotation - (turnInput * turnSpeed * forwardEffect * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);
        }

        // 2. DRIFT CORRECTION (The Fix)
        // We break velocity into "Forward Speed" and "Sideways Speed"
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);

        // We keep all the Forward Speed, but kill the Sideways Speed based on Drift Factor
        rb.linearVelocity = forwardVelocity + (rightVelocity * (1f - driftFactor));

        // 3. ACCELERATE
        if (Input.GetKey(KeyCode.W))
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
}