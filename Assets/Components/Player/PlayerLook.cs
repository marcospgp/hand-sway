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
    [Tooltip("The input action asset used to control the game character.")]
    private InputActionAsset inputActions;

    private InputActionMap looking;
    private InputAction lookInput;
    private Vector3 cameraEulerAngles = Vector3.zero;
    private Quaternion initialCameraRotation = Quaternion.identity;

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

        transform.rotation = initialCameraRotation;

        float horizontalDelta = mouseDelta.x * lookSensitivity * 0.01f;
        float verticalDelta = -1 * mouseDelta.y * lookSensitivity * 0.01f;

        cameraEulerAngles.x += verticalDelta;
        cameraEulerAngles.x = Mathf.Clamp(cameraEulerAngles.x, -90f, 90f);

        cameraEulerAngles.y += horizontalDelta;

        if (cameraEulerAngles.y > 360f) {
            cameraEulerAngles.y -= 360f;
        } else if (cameraEulerAngles.y < 360f) {
            cameraEulerAngles.y += 360f;
        }

        transform.Rotate(cameraEulerAngles, Space.Self);
    }

    private void LockCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
