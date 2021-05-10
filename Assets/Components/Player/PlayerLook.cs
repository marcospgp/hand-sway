using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerLook : MonoBehaviour {

    [SerializeField]
    [Tooltip("Affects how fast the camera moves when moving the mouse.")]
    private float lookSensitivity = 6f;

    [Header("References")]

    [SerializeField]
    new private Transform camera;

    [SerializeField]
    [Tooltip("The input action asset used to control the game character.")]
    private InputActionAsset inputActions;

    private InputActionMap looking;
    private InputAction lookInput;
    private Quaternion initialCameraRotation = Quaternion.identity;
    private float verticalAngle = 0f;
    private float horizontalAngle = 0f;

    public void Awake() {
        LockCursor();

        looking = inputActions.FindActionMap("Looking", throwIfNotFound: true);

        // Enable Looking action map by default
        looking.Enable();

        lookInput = inputActions.FindAction("Look", throwIfNotFound: true);
    }

    public void Update() {
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation() {
        Vector2 mouseDelta = lookInput.ReadValue<Vector2>();

        float horizontalDelta = mouseDelta.x * lookSensitivity * 0.01f;
        float verticalDelta = -1 * mouseDelta.y * lookSensitivity * 0.01f;

        verticalAngle += verticalDelta;
        verticalAngle = Mathf.Clamp(verticalAngle, -90f, 90f);

        horizontalAngle += horizontalDelta;

        if (horizontalAngle > 360f) {
            horizontalAngle -= 360f;
        } else if (horizontalAngle < -360f) {
            horizontalAngle += 360f;
        }

        camera.localRotation = Quaternion.Euler(verticalAngle, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, horizontalAngle, 0f);
    }

    private void LockCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
