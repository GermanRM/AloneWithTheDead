using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] private float cameraOffsetFromColliderTop = -0.15f;

    float xRotation;
    float yRotation;
    private Vector2 inputMouseDelta = Vector2.zero;

    [Header("Object References")]
    [SerializeField] private Transform cameraTransform = null;

    [Header("Script References")]
    [SerializeField] PlayerMovement playerMovement;
    private PlayerControls playerControls;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerControls = new PlayerControls();
        playerControls.Camera.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        if (cameraTransform == null)
        {
            Debug.LogError($"{GetType().Name}.LateUpdate(): cameraTransform reference is null - exiting early & disabling component", gameObject);
            this.enabled = false;
            return;
        }

        UpdateCameraRotation();
        UpdateCameraPosition();
    }

    private void GetInput()
    {
        inputMouseDelta = playerControls.Camera.CameraLook.ReadValue<Vector2>();
    }

    private void LateUpdate()
    {

    }

    private void UpdateCameraRotation()
    {
        float mouseX = inputMouseDelta.x * sensX;
        float mouseY = inputMouseDelta.y * sensY;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    private void UpdateCameraPosition()
    {
        cameraTransform.position = transform.position
            + playerMovement.GetCharacterController().center
            + new Vector3(0, 0.5f * playerMovement.GetCharacterController().height + cameraOffsetFromColliderTop, 0);
    }
}
