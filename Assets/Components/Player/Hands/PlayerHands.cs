using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerHands : MonoBehaviour {
    [SerializeField]
    [Range(0f, 1f)]
    private float cameraDeltaToSwayRatio = 0.9f;

    [SerializeField]
    [Tooltip(
        "How strongly the player's hands are pulled towards the camera's " +
        "orientation. This is multiplied by the angle between these two."
    )]
    [Range(0f, 300f)]
    private float swaySpringForce = 150f;

    [SerializeField]
    [Tooltip("Sway attenuation.")]
    [Range(1f, 50f)]
    private float swayDrag = 35f;

    [SerializeField]
    private ParticleSystem gunshotParticles;

    [Tooltip("References")]

    [SerializeField]
    private InputActionAsset inputActions;

    [SerializeField]
    private Transform playerCamera;

    private InputActionMap shootingActionMap;
    private InputAction shootInput;

    private GunSounds gunSounds;
    private Animator animator;
    private Vector3 angularVelocity = Vector3.zero;
    private Quaternion lastCameraRotation;
    private PlayerMovement playerMovement;

    public void Start() {
        gunSounds = GetComponentInChildren<GunSounds>();

        if (gunSounds == null) {
            throw new System.Exception("Missing GunSounds component.");
        }

        animator = GetComponent<Animator>();

        if (animator == null) {
            throw new System.Exception("Missing Animator component.");
        }

        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null) {
            throw new System.Exception("Missing PlayerMovement component.");
        }

        // Input

        shootingActionMap =
            inputActions.FindActionMap("Shooting", throwIfNotFound: true);

        shootingActionMap.Enable();

        shootInput = inputActions.FindAction("Shoot", throwIfNotFound: true);
    }

    public void Update() {
        UpdatePositionAndRotation();
        // Handle shooting & reloading

        if (shootInput.triggered) {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Reload")) {
                // Animation event triggers shooting, so that it is always
                // synchronized with animation and only happens if animation
                // happens
                animator.SetTrigger("Shoot");
            }
        }

        // Handle movement parameters

        if (playerMovement.IsWalking && !animator.GetBool("Walking")) {
            animator.SetBool("Walking", true);
        }

        if (!playerMovement.IsWalking && animator.GetBool("Walking")) {
            animator.SetBool("Walking", false);
        }

        if (playerMovement.IsRunning && !animator.GetBool("Running")) {
            animator.SetBool("Running", true);
        }

        if (!playerMovement.IsRunning && animator.GetBool("Running")) {
            animator.SetBool("Running", false);
        }
    }

    // Animation event callback
    public void ShotAnimationEvent() {
        gunshotParticles.Stop(
            withChildren: true,
            ParticleSystemStopBehavior.StopEmitting
        );

        gunshotParticles.Play(withChildren: true);

        gunSounds.Shoot();
    }

    private void UpdatePositionAndRotation() {
        // Apply drag
        angularVelocity *= Mathf.Max(0f, 1 - (swayDrag * Time.deltaTime));

        // Rotate hands by a fraction of camera movement

        Quaternion cameraDeltaRotation =
            Quaternion.Inverse(lastCameraRotation) * playerCamera.rotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            transform.rotation * cameraDeltaRotation,
            cameraDeltaToSwayRatio
        );

        // Rotate hands towards camera orientation

        // Rotation that goes from player hands to player camera
        Vector3 handDelta =
            (Quaternion.Inverse(transform.rotation) * playerCamera.rotation)
            .eulerAngles;

        // Ensure angles represent shortest path
        handDelta = NormalizeEulerAngles(handDelta);

        float angle = Quaternion.Angle(
            transform.rotation,
            playerCamera.rotation
        );

        float acceleration = swaySpringForce * angle;

        angularVelocity +=
            // Ensure delta rotation is in world space
            transform.TransformVector(handDelta).normalized *
            acceleration *
            Time.deltaTime;

        var rotation = transform.rotation *
            Quaternion.Euler(
                transform.InverseTransformVector(
                    angularVelocity * Time.deltaTime
                )
            );

        transform.SetPositionAndRotation(
            playerCamera.position,
            rotation
        );

        lastCameraRotation = playerCamera.rotation;
    }

    private Vector3 NormalizeEulerAngles(Vector3 angles) {
        static float NormalizeAngle(float angle) {
            if (angle > 180f) {
                return angle - 360f;
            } else if (angle < -180f) {
                return angle + 360f;
            } else {
                return angle;
            }
        }

        return new Vector3(
            NormalizeAngle(angles.x),
            NormalizeAngle(angles.y),
            NormalizeAngle(angles.z)
        );
    }
}
