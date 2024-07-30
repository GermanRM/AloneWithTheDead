using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float moveSpeedChange;
    [SerializeField] private float actualSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private bool isSprinting;
    private MovementState movementState;

    [Header("Jump Properties")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private bool canJump;
    [SerializeField] bool _jump;

    [Header("Crouch Properties")]
    [SerializeField] private Vector3 crouchScale;
    [SerializeField] private float crouchTime;
    [SerializeField] private bool isCrouching;
    [SerializeField] private float crouchCooldown;
    [SerializeField] private bool canCrouch;
    private Vector3 defaultLocalScale;

    [Header("GroundCheck Properties")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 groundCheckBoxSize;
    [SerializeField] private float groundRayDistance;
    [SerializeField] private LayerMask groundLayer;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity;
    [SerializeField] Vector3 velocity;
    [SerializeField] private float airControlReducer;
    bool wasInAir = false;

    [Header("Debug")]
    [SerializeField] private Vector2 movementInput;
    [SerializeField] private Vector3 moveDirection;
    [SerializeField] private bool Grounded;
    [SerializeField] private float magnitude;
    [SerializeField] private bool Moving;

    #region Events

    public event Action OnPlayerJump;
    public event Action OnPlayerLand;

    #endregion

    #region References

    private CharacterController controller;
    private PlayerControls playerControl;

    #endregion

    #region Getter / Setter

    /// <summary>
    /// Get is Sprinting Status
    /// </summary>
    /// <returns></returns>
    public bool GetIsSprinting() { return isSprinting; }

    public MovementState GetMovementState() { return movementState; }

    public CharacterController GetPlayerCC() { return controller; }

    #endregion

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerControl = new PlayerControls();
        playerControl.Movement.Enable();

        defaultLocalScale = transform.localScale;
    }

    void Update()
    {
        GetInputs();
        Movement();
        Jump();
        Crouch();
        Sprint();
        SpeedManager();
        ApplyGravity();

        //Debug
        magnitude = controller.velocity.magnitude;
        Moving = isMoving();
        Grounded = isGrounded();
    }

    #region Inputs

    /// <summary>
    /// Get Player Inputs
    /// </summary>
    private void GetInputs()
    {
        movementInput = playerControl.Movement.Movement.ReadValue<Vector2>();

        _jump = playerControl.Movement.Jump.IsInProgress();
    }

    #endregion

    #region Movement

    private void Sprint()
    {
        if (playerControl.Movement.Sprint.IsPressed() && isMovingForward(movementInput))
        {
            if (isCrouching) return;

            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    private void SpeedManager()
    {
        if (isCrouching)
        {
            maxSpeed = crouchSpeed;
            movementState = MovementState.crouching;
            actualSpeed = Mathf.Lerp(actualSpeed, crouchSpeed, moveSpeedChange * Time.deltaTime);
        }
        else
        {
            if (isSprinting)
            {
                maxSpeed = runSpeed;
                movementState = MovementState.sprinting;
                actualSpeed = Mathf.Lerp(actualSpeed, runSpeed, moveSpeedChange * Time.deltaTime);
            }
            else
            {
                maxSpeed = walkSpeed;
                movementState = MovementState.walking;
                actualSpeed = Mathf.Lerp(actualSpeed, walkSpeed, moveSpeedChange * Time.deltaTime);
            }
        }
    }

    private void Movement()
    {
        if (movementInput != Vector2.zero)
        {
            Vector2 movement = movementInput.normalized;

            moveDirection = (transform.forward * movement.y + transform.right * movement.x) * actualSpeed;

            if (isGrounded())
            {
                controller.Move(moveDirection * Time.deltaTime);
            }
            else
            {
                controller.Move(moveDirection * airControlReducer * Time.deltaTime);
            }
        }
    }

    private IEnumerator jumpCoroutine()
    {
        yield return new WaitForSeconds(jumpCooldown);

        canJump = true;
    }

    private void Jump()
    {     
        if (_jump && isGrounded() && canJump)
        {
            OnPlayerJump?.Invoke();
            velocity.y = Mathf.Sqrt(jumpForce * -5f * gravity);
            canJump = false;
            _jump = false;

            StartCoroutine(jumpCoroutine());
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded() && velocity.y < 1)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        if (wasInAir && isGrounded())
        {
            OnPlayerLand?.Invoke();
        }

        wasInAir = !isGrounded();
    }

    private IEnumerator CrouchCoroutine()
    {
        yield return new WaitForSeconds(crouchCooldown);
        canCrouch = true;
    }

    private void Crouch()
    {
        if (playerControl.Movement.Crouch.WasPressedThisFrame() && canCrouch)
        {
            isCrouching = true;
            canCrouch = false;
        }

        if (playerControl.Movement.Crouch.WasReleasedThisFrame() && isCrouching)
        {
            isCrouching = false;

            StartCoroutine(CrouchCoroutine());
        }

        if (isCrouching)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, crouchScale, crouchTime * Time.deltaTime);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultLocalScale, crouchTime * Time.deltaTime);
        }
    }

    #endregion

    #region Checkers

    /// <summary>
    /// Detect if player is moving forward
    /// </summary>
    /// <param name="movement"></param>
    /// <returns></returns>
    public bool isMovingForward(Vector2 movement)
    {
        return movement.y > 0 && controller.velocity.magnitude > 1f;
    }

    /// <summary>
    /// Detect if player is moving
    /// </summary>
    /// <returns></returns>
    public bool isMoving()
    {      
        return movementInput != Vector2.zero && controller.velocity.magnitude > 1;
    }

    /// <summary>
    /// Detect if player is grounded
    /// </summary>
    /// <returns></returns>
    public bool isGrounded()
    {
        return gameObject.scene.GetPhysicsScene().BoxCast(groundCheck.position, groundCheckBoxSize, Vector3.down, out RaycastHit hit, Quaternion.identity, groundRayDistance, groundLayer);
    }

    #endregion

    #region Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckBoxSize); //Draw box check
    }
    #endregion
}
