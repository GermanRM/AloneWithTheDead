using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.SocialPlatforms;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input Settings")]
    private bool isRunInputDown = false;
    private bool isCrouchInputDown = false;
    private bool isJumpInputDown = false;
    [SerializeField] private bool useToggleInputForSprint = false;
    [SerializeField] private bool useToggleInputForCrouch = true;

    [Header("Movement Settings")]
    [SerializeField, Min(0f)] private float moveSpeedWalk = 1.9f;
    [SerializeField, Min(0f)] private float moveSpeedRun = 3.5f;
    [SerializeField, Min(0f)] private float moveSpeedCrouch = 1.6f;

    [Header("GroundCheck Properties")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 groundCheckBoxSize;
    [SerializeField] private float groundRayDistance;
    [SerializeField] private LayerMask groundLayer;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchColliderHeight = 1.0f;
    [SerializeField, Min(0f)] private float crouchSmoothTime = 0.2f;
    [SerializeField, Min(0f)] private float standUpSmoothTime = 0.15f;
    [SerializeField] private LayerMask aboveFreeSpaceBlockingLayers;

    [Header("Jump Settings")]
    [SerializeField] private bool cancelCrouchWithJump = true;
    [SerializeField, Min(0)] private float minJumpTime = 0.1f;
    [SerializeField, Min(0)] private float maxJumpTime = 0.3f;
    [SerializeField, Min(0)] private float jumpForce = 11.0f;
    [SerializeField] private AnimationCurve jumpForceCurveOverJumpTime = new AnimationCurve() { keys = new Keyframe[] { new Keyframe { time = 0, value = 1 }, new Keyframe() { time = 1, value = 0 }, } };
    [SerializeField, Min(0)] private float jumpCooldown = 0.4f;
    [SerializeField] private float fallGravity = Physics.gravity.y;

    [Header("Object References")]
    [SerializeField] private Transform cameraTransform = null;

    private bool isRunning = false;

    private bool isCrouching = false;
    private bool isCrouchingUnderObject = false;

    private bool isJumping = false;
    private float jumpStartTime;
    private float jumpVelocity = 0f;

    private float cameraPitch;
    private float cameraYaw;

    private float defaultColliderHeight;
    private float colliderHeightVelocity;

    private Vector2 movementInput = Vector2.zero;

    private Collider[] overlapColliders = new Collider[8];

    #region Events

    public event Action OnPlayerJump;
    public event Action OnPlayerLand;

    #endregion

    #region References

    private CharacterController characterController;
    private PlayerControls playerControl;

    #endregion

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerControl = new PlayerControls();
        playerControl.Movement.Enable();

        cameraTransform = Camera.main.transform;
        defaultColliderHeight = characterController.height;
    }

    #region Getter / Setter

    public CharacterController GetCharacterController() { return characterController; }

    public bool GetIsRunning() { return isRunning; }

    public float GetMoveVelocityMagnitude()
    {
        if (isGrounded() == false)
            return 0f;

        var velocity = characterController.velocity;
        velocity.y = 0f;
        return velocity.magnitude;
    }

    public bool IsCrouching()
    {
        if (isGrounded() == false)
            return false;

        return isCrouching;
    }

    public bool IsRunningAndMoving()
    {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0f;
        return isRunning && velocity.sqrMagnitude > 0.1f;
    }

    #endregion

    private void Update()
    {
        isCrouchingUnderObject = false;

        if (isCrouching)
            CheckForAboveObject();

        GetInputs();
        UpdateColliderHeight();
        UpdateTransform();
    }

    #region Inputs

    /// <summary>
    /// Get Player Inputs
    /// </summary>
    private void GetInputs()
    {
        bool runHeldPreviously = isRunInputDown;
        bool crouchHeldPreviously = isCrouchInputDown;
        bool jumpHeldPreviously = isJumpInputDown;

        isRunInputDown = playerControl.Movement.Sprint.IsInProgress();
        isCrouchInputDown = playerControl.Movement.Crouch.IsInProgress();
        isJumpInputDown = playerControl.Movement.Jump.IsInProgress();

        //isRunning = isRunInputDown;

        // cancel crouch by running
        if (isRunning && isCrouching)
            isCrouching = false;

        // update current crouch state
        if (useToggleInputForCrouch)
        {
            if (crouchHeldPreviously == false && isCrouchInputDown)
                isCrouching = !isCrouching;
        }
        else
            isCrouching = isCrouchInputDown;

        // override crouch if under object
        if (isCrouchingUnderObject)
            isCrouching = true;

        // cancel running by crouch
        if (isCrouching && isRunning)
            isRunning = false;

        UpdateJumpState(jumpHeldPreviously);

        movementInput = playerControl.Movement.Movement.ReadValue<Vector2>();
        movementInput = Vector2.ClampMagnitude(movementInput, 1.0f);
    }

    private void UpdateJumpState(bool jumpHeldPreviously)
    {
        if (isJumpInputDown)
        {
            if (isCrouching)
            {
                // jump button pressed this frame and can stand up
                if (jumpHeldPreviously == false && cancelCrouchWithJump && isCrouchingUnderObject == false)
                    isCrouching = false;
            }
            else
            {
                if (isJumping)
                {
                    float timeSpentJumping = Time.time - jumpStartTime;

                    if (timeSpentJumping > maxJumpTime)
                        isJumping = false;
                }
                else
                {
                    // jump started this frame
                    if (jumpHeldPreviously == false && CanJump())
                    {
                        jumpStartTime = Time.time;
                        isJumping = true;
                        jumpVelocity = jumpForce;
                    }
                }
            }
        }
        else
        {
            if (isJumping)
            {
                float timeSpentJumping = Time.time - jumpStartTime;
                if (timeSpentJumping >= minJumpTime)
                    isJumping = false;
            }
        }
    }

    #endregion

    #region Movement

    private void UpdateTransform()
    {
        Vector3 cameraHorizontalForward = cameraTransform.forward;
        cameraHorizontalForward.y = 0;
        cameraHorizontalForward.Normalize();

        if (cameraHorizontalForward != Vector3.zero)
            transform.forward = cameraHorizontalForward;

        float moveSpeed = moveSpeedWalk;

        isRunning = isRunInputDown && isMovingForward(movementInput);

        if (isRunning)
            moveSpeed = moveSpeedRun;

        if (isCrouching)
            moveSpeed = moveSpeedCrouch;

        Vector3 moveVector = Time.deltaTime * moveSpeed * (movementInput.x * transform.right + movementInput.y * transform.forward);
        moveVector.y = Time.deltaTime * fallGravity;

        ApplyJumpVelocity(ref moveVector);

        characterController.Move(moveVector);
    }

    private void ApplyJumpVelocity(ref Vector3 moveVector)
    {
        if (jumpVelocity <= 0)
            return;

        float timeSpentJumping = Time.time - jumpStartTime;
        float jumpTime = Mathf.Clamp01(timeSpentJumping / maxJumpTime);
        float jumpForceScale = jumpForceCurveOverJumpTime.Evaluate(jumpTime);

        jumpVelocity += Time.deltaTime * jumpForceScale * jumpForce;
        jumpVelocity += Time.deltaTime * fallGravity;

        if (jumpVelocity < 0)
            jumpVelocity = 0;

        moveVector.y += Time.deltaTime * jumpVelocity;
    }

    private void UpdateColliderHeight()
    {
        float targetHeight = isCrouching ? crouchColliderHeight : defaultColliderHeight;
        float smoothTime = isCrouching ? crouchSmoothTime : standUpSmoothTime;
        characterController.height = Mathf.SmoothDamp(characterController.height, targetHeight, ref colliderHeightVelocity, smoothTime, float.MaxValue, Time.deltaTime);
        characterController.center = new Vector3(0, 0.5f * characterController.height + characterController.stepOffset, 0);
    }

    #endregion

    #region Checkers

    public bool IsMoving(float minimunMove)
    {
        return characterController.velocity.sqrMagnitude > minimunMove;
    }

    /// <summary>
    /// Detect if player is moving forward
    /// </summary>
    /// <param name="movement"></param>
    /// <returns></returns>
    public bool isMovingForward(Vector2 movement)
    {
        return movement.y > 0 && characterController.velocity.magnitude > 1f;
    }

    /// <summary>
    /// Detect if player is grounded
    /// </summary>
    /// <returns></returns>
    public bool isGrounded()
    {
        return gameObject.scene.GetPhysicsScene().BoxCast(groundCheck.position, groundCheckBoxSize, Vector3.down, out RaycastHit hit, Quaternion.identity, groundRayDistance, groundLayer);
    }

    private bool CanJump()
    {
        if (Time.time - jumpStartTime < jumpCooldown)
            return false;

        return isCrouching == false && isGrounded();
    }

    private void CheckForAboveObject()
    {
        const float upCheckDistance = 0.05f;
        const float checkRadiusReduceAmount = 0.02f;

        float checkRadius = characterController.radius - checkRadiusReduceAmount;

        Vector3 checkPosition = transform.position
            + characterController.center
            + new Vector3(0, characterController.height - checkRadius + upCheckDistance, 0);

        int overlapCount = Physics.OverlapSphereNonAlloc(
            checkPosition,
            checkRadius,
            overlapColliders,
            aboveFreeSpaceBlockingLayers,
            QueryTriggerInteraction.Ignore);

        if (overlapCount == 0)
            return;

        for (int i = 0; i < overlapCount; i++)
        {
            var overlapCollider = overlapColliders[i];

            if (overlapCollider.transform.GetInstanceID() == characterController.transform.GetInstanceID())
                continue;

            isCrouchingUnderObject = true;
            return;
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        ExtDebug.DrawBoxCastBox(groundCheck.position, groundCheckBoxSize, Quaternion.identity, Vector3.down, groundRayDistance, isGrounded() ? Color.red : Color.green);
    }

    #endregion
}
