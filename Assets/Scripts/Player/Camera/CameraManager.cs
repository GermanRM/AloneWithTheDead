using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform cameraHolderTransform;

    private void Awake()
    {
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        AssignCameraPosition();
    }

    private void AssignCameraPosition()
    {
        mainCamera.transform.position = cameraHolderTransform.position;
    }

}
