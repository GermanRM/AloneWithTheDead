using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Camera Properties")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform cameraHolderTransform;

    [Header("Camera FOV")]
    [SerializeField] private float walkFOV;
    [SerializeField] private float runFOV;
    [SerializeField] private float actualFOV;
    [SerializeField] private float fovChangeSpeed;

    [Header("Mouse Properties")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] private Vector2 mouseLook;
    float xRotation;
    float yRotation;

    [Header("Scripts References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Space]

    #region References
    [SerializeField ] private Transform playerTransform;
    private PlayerControls playerControls;
    #endregion

    #region Getter / Setter

    public Vector2 GetSens() { return new Vector2(sensX, sensY); }

    public void SetSens(Vector2 sens)
    {
        sensX = sens.x;
        sensY = sens.y;
    }

    #endregion

    private void Start()
    {
        playerControls = new PlayerControls();
        playerControls.Camera.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        DynamicFOV();
        CameraLook();
        RotatePlayer();
    }

    #region Inputs

    /// <summary>
    /// Get Player Inputs
    /// </summary>
    private void GetInputs()
    {
        mouseLook = playerControls.Camera.CameraLook.ReadValue<Vector2>();
    }

    #endregion

    #region Camera Movement

    /// <summary>
    /// Move Camera by players input
    /// </summary>
    private void CameraLook()
    {
        if (mouseLook != Vector2.zero)
        {
            float mouseX = mouseLook.x * sensX;
            float mouseY = mouseLook.y * sensY;

            xRotation -= mouseY;
            yRotation += mouseX;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cameraHolderTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            mainCamera.transform.rotation = cameraHolderTransform.rotation;
        }
    }

    /// <summary>
    /// Change FOV dynamically by the sprint status
    /// </summary>
    private void DynamicFOV()
    {
        if (playerMovement.GetIsSprinting())
            actualFOV = Mathf.Lerp(actualFOV, runFOV, fovChangeSpeed * Time.deltaTime);
        else
            actualFOV = Mathf.Lerp(actualFOV, walkFOV, fovChangeSpeed * Time.deltaTime);

        mainCamera.fieldOfView = actualFOV;
    }

    #endregion

    private void RotatePlayer()
    {
        playerTransform.rotation = Quaternion.Euler(0, yRotation, 0f);
    }
}
