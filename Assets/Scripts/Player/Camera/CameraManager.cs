using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] private float aimingMultiplier;
    [SerializeField] private float cameraOffsetFromColliderTop = -0.15f;

    [Header("Fov Properties")]
    [SerializeField] private float defaultFOV;
    [SerializeField] private float sprintFOV;
    [SerializeField] private float sprintFOVChangeSpeed;
    [SerializeField] private float aimingFOV;
    [SerializeField] private float aimingFOVChangeSpeed;

    float xRotation;
    float yRotation;
    private Vector2 inputMouseDelta = Vector2.zero;

    [Header("Object References")]
    [SerializeField] private Transform cameraTransform = null;

    [Header("Script References")]
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] PlayerStats playerStats;
    private PlayerControls playerControls;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
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

        SprintFOV();
        AimingFOV();
    }

    #region Inputs

    private void GetInput()
    {
        inputMouseDelta = playerControls.Camera.CameraLook.ReadValue<Vector2>();
    }

    #endregion

    #region Camera Movement

    private void UpdateCameraRotation()
    {
        float mouseX = 0;
        float mouseY = 0;

        if (!playerStats.isAiming)
        {
            mouseX = inputMouseDelta.x * sensX;
            mouseY = inputMouseDelta.y * sensY;
        }
        else
        {
            mouseX = inputMouseDelta.x * sensX * aimingMultiplier;
            mouseY = inputMouseDelta.y * sensY * aimingMultiplier;
        }

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

    #endregion

    #region FOV Managagement

    /// <summary>
    /// Change FOV with lerp
    /// </summary>
    /// <param name="newFov"></param>
    /// <param name="speed"></param>
    public void ChangeFovDynamically(float newFov, float speed)
    {
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, newFov, speed * Time.deltaTime);
    }

    private void SprintFOV()
    {
        if (playerMovement.IsRunningAndMoving()) ChangeFovDynamically(sprintFOV, sprintFOVChangeSpeed);
        else ChangeFovDynamically(defaultFOV, sprintFOVChangeSpeed);
    }

    private void AimingFOV()
    {
        if (playerStats.isAiming) ChangeFovDynamically(aimingFOV, aimingFOVChangeSpeed);
        else ChangeFovDynamically(defaultFOV, aimingFOVChangeSpeed);
    }

    #endregion
}
