using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Tensori.FPSHandsHorrorPack
{
    [RequireComponent(typeof(CharacterController))]
    [DefaultExecutionOrder(-1)]
    public class FPSCharacterController : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private string inputAxis_MoveHorizontal = "Horizontal";
        [SerializeField] private string inputAxis_MoveVertical = "Vertical";
        [SerializeField] private string inputAxis_MouseX = "Mouse X";
        [SerializeField] private string inputAxis_MouseY = "Mouse Y";

        [SerializeField] private KeyCode inputKeySprint = KeyCode.LeftShift;
        [SerializeField] private KeyCode inputKeyCrouch = KeyCode.LeftControl;
        [SerializeField] private KeyCode inputKeyJump = KeyCode.Space;

        [SerializeField] private bool useToggleInputForSprint = false;
        [SerializeField] private bool useToggleInputForCrouch = true;

        [Header("Movement Settings")]
        [SerializeField, Min(0f)] private float moveSpeedWalk = 1.9f;
        [SerializeField, Min(0f)] private float moveSpeedRun = 3.5f;
        [SerializeField, Min(0f)] private float moveSpeedCrouch = 1.6f;

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
        [SerializeField] private AnimationCurve jumpForceCurveOverJumpTime = new AnimationCurve() { keys = new Keyframe[] { new Keyframe { time = 0, value = 1 }, new Keyframe() { time = 1, value = 0 }, }};
        [SerializeField, Min(0)] private float jumpCooldown = 0.4f;
        [SerializeField] private float fallGravity = Physics.gravity.y;

        [Header("Camera Settings")]
        [SerializeField] private float cameraOffsetFromColliderTop = -0.15f;
        [SerializeField] private float cameraMouseSensitivity = 1f;
        [SerializeField] private float cameraMinPitch = 0f;
        [SerializeField] private float cameraMaxPitch = 0f;

        [Header("Object References")]
        [SerializeField] private Transform cameraTransform = null;

        private bool isRunInputDown = false;
        private bool isCrouchInputDown = false;
        private bool isJumpInputDown = false;

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
        private Vector2 inputMouseDelta = Vector2.zero;

        private CharacterController characterController = null;
        private Collider[] overlapColliders = new Collider[8];

        private void Awake()
        {
            TryGetComponent(out characterController);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            defaultColliderHeight = characterController.height;

            if (cameraTransform != null)
            {
                Vector3 cameraEuler = cameraTransform.rotation.eulerAngles;
                cameraPitch = cameraEuler.x;
                cameraYaw = cameraEuler.y;
            }
        }

        private void Update()
        {
            if (cameraTransform == null)
            {
                Debug.LogError($"{GetType().Name}.Update(): cameraTransform reference is null - exiting early & disabling component", gameObject);
                this.enabled = false;
                return;
            }

            isCrouchingUnderObject = false;

            if (isCrouching)
                CheckForAboveObject();

            UpdateInput();
            UpdateColliderHeight();
            UpdateTransform();
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

        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                Debug.LogError($"{GetType().Name}.LateUpdate(): cameraTransform reference is null - exiting early & disabling component", gameObject);
                this.enabled = false;
                return;
            }

            UpdateCameraRotation();
            UpdateCameraPosition();
        }

        private void UpdateInput()
        {
            bool runHeldPreviously = isRunInputDown;
            bool crouchHeldPreviously = isCrouchInputDown;
            bool jumpHeldPreviously = isJumpInputDown;

            isRunInputDown = Input.GetKey(inputKeySprint);
            isCrouchInputDown = Input.GetKey(inputKeyCrouch);
            isJumpInputDown = Input.GetKey(inputKeyJump);

            // update current running state
            if (useToggleInputForSprint)
            {
                if (runHeldPreviously == false && isRunInputDown)
                    isRunning = !isRunning;
            }
            else
                isRunning = isRunInputDown;

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

            movementInput = new Vector2(Input.GetAxis(inputAxis_MoveHorizontal), Input.GetAxis(inputAxis_MoveVertical));
            movementInput = Vector2.ClampMagnitude(movementInput, 1.0f);

            inputMouseDelta = new Vector2(Input.GetAxis(inputAxis_MouseX), Input.GetAxis(inputAxis_MouseY));
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

        private bool CanJump()
        {
            if (Time.time - jumpStartTime < jumpCooldown)
                return false;

            return isCrouching == false && characterController.isGrounded;
        }

        private void UpdateColliderHeight()
        {
            float targetHeight = isCrouching ? crouchColliderHeight : defaultColliderHeight;
            float smoothTime = isCrouching ? crouchSmoothTime : standUpSmoothTime;
            characterController.height = Mathf.SmoothDamp(characterController.height, targetHeight, ref colliderHeightVelocity, smoothTime, float.MaxValue, Time.deltaTime);
            characterController.center = new Vector3(0, 0.5f * characterController.height + characterController.stepOffset, 0);
        }

        private void UpdateCameraRotation()
        {
            cameraYaw += inputMouseDelta.x * cameraMouseSensitivity;
            cameraPitch += -inputMouseDelta.y * cameraMouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, cameraMinPitch, cameraMaxPitch);

            if (cameraYaw < -180) cameraYaw += 360f;
            if (cameraYaw > 180f) cameraYaw -= 360f;

            if (cameraPitch < -180f) cameraPitch += 360f;
            if (cameraPitch > 180f) cameraPitch -= 360f;

            cameraTransform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        }

        private void UpdateCameraPosition()
        {
            cameraTransform.position = transform.position 
                + characterController.center 
                + new Vector3(0, 0.5f * characterController.height + cameraOffsetFromColliderTop, 0);
        }

        private void UpdateTransform()
        {
            Vector3 cameraHorizontalForward = cameraTransform.forward;
            cameraHorizontalForward.y = 0;
            cameraHorizontalForward.Normalize();

            if (cameraHorizontalForward != Vector3.zero)
                transform.forward = cameraHorizontalForward;

            float moveSpeed = moveSpeedWalk;

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

        public bool IsRunningAndMoving()
        {
            Vector3 velocity = characterController.velocity;
            velocity.y = 0f;
            return isRunning && velocity.sqrMagnitude > 0.1f;
        }

        public bool IsCrouching()
        {
            if (characterController.isGrounded == false)
                return false;

            return isCrouching;
        }

        public float GetMoveVelocityMagnitude()
        {
            if (characterController.isGrounded == false)
                return 0f;

            var velocity = characterController.velocity;
            velocity.y = 0f;
            return velocity.magnitude;
        }
    }
}