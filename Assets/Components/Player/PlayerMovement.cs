using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement")]

    [SerializeField]
    private float walkForce = 80f;

    [SerializeField]
    private float sprintForce = 160f;

    [SerializeField]
    private float airWalkForce = 2f;

    [SerializeField]
    private float groundFriction = 20f;

    [SerializeField]
    [Tooltip("Instant Y axis velocity added to player when jumping.")]
    private float jumpImpulse = 6f;

    [Header("Settings")]

    [SerializeField]
    [Tooltip(
        "Layermask used to check that the player is touching the ground " +
        "(should include everything except the player)."
    )]
    private LayerMask groundLayerMask;

    [Header("References")]

    [SerializeField]
    new private CapsuleCollider collider;

    [SerializeField]
    [Tooltip("The input action asset used to control the game character.")]
    private InputActionAsset inputActions;

    private InputActionMap walking;

    private InputAction move;
    private InputAction sprint;
    private InputAction jump;

    new private Rigidbody rigidbody;

    private Collider ground;
    private bool isGrounded;

    // Needed to avoid player jumping twice before coyote time ends
    private float timeSinceLastJumped = 9999f;
    private float timeSinceLastGrounded = 9999f;

    public bool IsWalking { get; private set; }
    public bool IsRunning { get; private set; }

    public void Awake() {
        // Components required by OnAnimatorMove() must be initialized in
        // Awake() because Start() runs after it.
        // Also this component may be disabled when joining a room, and this
        // needs to run regardless.

        if (!TryGetComponent(out rigidbody)) {
            throw new System.Exception("Missing Rigidbody component.");
        }

        walking = inputActions.FindActionMap("Walking", throwIfNotFound: true);

        // Enable walking action map by default
        walking.Enable();

        move = inputActions.FindAction("Move", throwIfNotFound: true);
        sprint = inputActions.FindAction("Sprint", throwIfNotFound: true);
        jump = inputActions.FindAction("Jump", throwIfNotFound: true);
    }

    public void Update() {
        Jump();
    }

    public void FixedUpdate() {
        GroundCheck();

        Vector2 movementInput = GetMovementInput();

        Move(movementInput);
    }

    private void Move(Vector2 movementInput) {
        bool isSprinting = sprint.ReadValue<float>() >= 0.5;

        // Update public movement properties
        if (movementInput.magnitude > 0) {
            if (isSprinting) {
                IsWalking = false;
                IsRunning = true;
            } else {
                IsWalking = true;
                IsRunning = false;
            }
        } else {
            IsWalking = false;
            IsRunning = false;
        }

        // Move player

        Vector3 direction = transform.TransformDirection(
            new Vector3(movementInput.x, 0f, movementInput.y)
        ).normalized;

        float acceleration;

        if (!isGrounded) {
            acceleration = airWalkForce;
        } else if (isSprinting) {
            acceleration = sprintForce;
        } else {
            acceleration = walkForce;
        }

        rigidbody.AddForce(
            direction * acceleration,
            ForceMode.Acceleration
        );

        // If we are grounded, apply resistance to movement
        // (player physics material should have no friction so it doesn't
        // stick to walls)
        if (isGrounded) {
            // Allow player to walk on moving ground, such as vehicles
            if (ground != null && ground.attachedRigidbody != null) {
                rigidbody.velocity = Vector3.Lerp(
                    rigidbody.velocity,
                    ground.attachedRigidbody.velocity,
                    Time.deltaTime * groundFriction
                );
            } else if (rigidbody.velocity.magnitude > 0f) {
                rigidbody.velocity -=
                    rigidbody.velocity * Time.deltaTime * groundFriction;
            }
        }
    }

    private void Jump() {
        if (isGrounded) {
            timeSinceLastGrounded = 0f;
        } else {
            timeSinceLastGrounded += Time.deltaTime;
        }

        if (
            jump.triggered &&
            timeSinceLastGrounded < 0.3f && // Coyote time
            timeSinceLastJumped > 0.3f // Prevent double jumps
        ) {
            rigidbody.AddForce(
                Vector3.up * jumpImpulse, ForceMode.VelocityChange
            );

            timeSinceLastJumped = 0f;
        }

        timeSinceLastJumped += Time.deltaTime;
    }

    private void GroundCheck() {
        Vector3 capsuleBottomSphereCenter =
            collider.bounds.center +
            collider.transform.up * -1f * (collider.height / 2f) +
            collider.transform.up * collider.radius;

        // We check a sphere that is slightly smaller than the player's collider
        // and move it down a little so it doesn't detect ground on the sides
        Collider[] grounds = Physics.OverlapSphere(
            capsuleBottomSphereCenter + (collider.transform.up * -0.02f),
            collider.radius - 0.01f,
            groundLayerMask
        );

        isGrounded = grounds.Length > 0;

        if (grounds.Length > 0) {
            ground = grounds[0];
        } else {
            ground = null;
        }
    }

    private Vector2 GetMovementInput() {
        Vector2 movement = move.ReadValue<Vector2>().normalized;

        // Process input so that smaller values have higher impact
        // (input values of -1, 0, and 1 remain unchanged, only values
        // in between are modulated).
        // Mathf.Pow returns NaN on negative numbers, so we use Mathf.Abs and
        // invert later if necessary.
        static float F(float x) {
            var y = Mathf.Pow(Mathf.Abs(x), 1f/2f);

            if (x < 0) {
                y *= -1;
            }

            return y;
        }

        return new Vector2(F(movement.x), F(movement.y));
    }
}
