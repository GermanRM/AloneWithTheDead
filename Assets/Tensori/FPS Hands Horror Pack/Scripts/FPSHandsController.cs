using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace Tensori.FPSHandsHorrorPack
{
    public class FPSHandsController : MonoBehaviour
    {
        public bool IsAttacking => attackCoroutine != null;
        public bool IsReloading => reloadCoroutine != null;
        public bool IsChangingItem => changeItemCoroutine != null;

        [Tooltip("Current active item asset")]
        [SerializeField] private FPSItem heldItem = null;

        [Header("Runtime Parameters - can be updated with other scripts")]
        public bool CanShoot = true;
        public bool CanReload = true;

        [Header("Input Settings")]
        [SerializeField] private KeyCode aimKey = KeyCode.Mouse1;
        [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode reloadKey = KeyCode.R;
        public bool IsAiming = false;

        [Header("Animator Settings")]
        [Tooltip("Name of the animator parameter that is used to control the playback position of animation states from the hands' animator component.")]
        [SerializeField] private string handsAnimatorTimeParameter = "AnimationTime";

        [Header("Object References")]
        [SerializeField] private FPSCharacterController fpsCharacterController = null;
        [SerializeField] private Transform handsParentTransform = null;
        [SerializeField] private Transform handsTransform = null;
        [SerializeField] private Animator handsAnimator = null;

        [Header("Events")]
        public UnityEvent<string> OnAnimationEvent = new UnityEvent<string>();

        private int currentAttackAnimationIndex = 0;

        private float poseAnimationTimer = 0f;

        private float equipPoseBlend = 0f;
        private float unequipPoseBlend = 0f;

        private Vector2 movementBouncePositionOffset = Vector2.zero;

        private Vector3 handsPositionOffset = Vector3.zero;
        private Vector3 handsPositionVelocity = Vector3.zero;
        private Vector3 handsEulerOffset = Vector3.zero;
        private Vector3 handsEulerVelocity = Vector3.zero;

        private string currentHandsAnimationState = null;
        private string currentItemAnimationState = null;

        private FPSItem heldItemPreviousFrame = null;
        private Transform heldItemTransform = null;
        private Animator heldItemAnimator = null;
        private FPSItem.ItemPose currentHandsPose = null;
        private FPSItem.AnimatedItemPose currentAnimatedPose = null;

        private Coroutine attackCoroutine = null;
        private Coroutine reloadCoroutine = null;
        private Coroutine changeItemCoroutine = null;

        private List<Transform> handsChildTransforms = new List<Transform>();
        private List<int> triggeredAnimationEvents = new List<int>();
        private List<FPSItem.AnimationEvent> animationEventsList = new List<FPSItem.AnimationEvent>();

        private void LateUpdate()
        {
            ValidateHeldItem();
            UpdateInput();
            UpdateActivePoses();
            UpdateMovementBounce(deltaTime: Time.deltaTime);
            UpdateHandsPosition();

            if (IsAttacking || IsReloading || currentHandsPose == null)
                return;

            PlayHandsAnimation(currentHandsPose.HandsAnimationStateName, currentHandsPose.AnimationStateBlendTime);
            PlayItemAnimation(currentHandsPose.ItemAnimationStateName, currentHandsPose.AnimationStateBlendTime);
        }

        private void ValidateHeldItem()
        {
            if (heldItemPreviousFrame == heldItem)
                return;

            StopActiveCoroutines(allowWeaponChangeRoutine: true);

            if (heldItemTransform != null)
                Destroy(heldItemTransform.gameObject);

            heldItemTransform = null;
            heldItemAnimator = null;
            currentHandsAnimationState = null;
            currentItemAnimationState = null;

            heldItemPreviousFrame = heldItem;

            if (heldItem == null || heldItem.ItemPrefab == null)
                return;

            GameObject spawnedItem = Instantiate(heldItem.ItemPrefab, parent: null);
            heldItemTransform = spawnedItem.transform;
            heldItemAnimator = heldItemTransform.GetComponentInChildren<Animator>(includeInactive: true);

            if (heldItemAnimator == null)
                Debug.LogWarning($"{GetType().Name}.validateHeldItem(): spawned item object '{heldItemTransform.name}' doesn't have an animator component", spawnedItem);

            Transform handsPivotBoneTransform = GetHandsBoneTransform(heldItem.HandsPivotBoneTransformName);

            if (handsPivotBoneTransform != null)
                heldItemTransform.SetParent(handsPivotBoneTransform, false);
            else
                Debug.LogWarning($"{GetType().Name}.validateHeldItem(): hands pivot bone not found with name {heldItem.HandsPivotBoneTransformName}", gameObject);
        }

        private void UpdateInput()
        {
            if (heldItem == null)
                return;

            if (Input.GetKeyDown(shootKey) && CanShoot_Internal())
            {
                StopActiveCoroutines(allowWeaponChangeRoutine: false);

                attackCoroutine = StartCoroutine(Coroutine_UpdateAttackAnimation(currentAttackAnimationIndex));

                currentAttackAnimationIndex++;

                if (currentAttackAnimationIndex >= heldItem.AttackAnimations.Count)
                    currentAttackAnimationIndex = 0;
            }

            if (Input.GetKeyDown(reloadKey) && CanReload_Internal())
            {
                StopActiveCoroutines(allowWeaponChangeRoutine: false);

                currentAttackAnimationIndex = 0;

                reloadCoroutine = StartCoroutine(Coroutine_UpdateAnimatedPose(heldItem.ReloadPose));
            }

            if (Input.GetKeyDown(aimKey))
                IsAiming = true;
            if (Input.GetKeyUp(aimKey))
                IsAiming = false;
        }

        private bool CanShoot_Internal()
        {
            if (CanShoot == false)
                return false;

            if (heldItem == null)
                return false;

            if (heldItem.AttackAnimations.Count == 0)
                return false;

            if (IsReloading)
                return heldItem.CanCancelReloadAnimationByShooting;

            if (IsChangingItem)
                return false;

            return true;
        }

        private bool CanReload_Internal()
        {
            if (heldItem == null)
                return false;

            if (CanReload == false)
                return false;

            if (IsReloading)
                return false;

            if (IsChangingItem)
                return false;

            return true;
        }

        public bool CanChangeItem()
        {
            if (heldItem != null)
            {
                if (IsReloading)
                    return heldItem.CanCancelReloadAnimationByChangingItem;
            }

            if (IsChangingItem)
                return false;

            return true;
        }

        private void StopActiveCoroutines(bool allowWeaponChangeRoutine)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }

            if (allowWeaponChangeRoutine == false)
            {
                if (changeItemCoroutine != null)
                {
                    StopCoroutine(changeItemCoroutine);
                    changeItemCoroutine = null;
                }

                equipPoseBlend = 0;
                unequipPoseBlend = 0;
            }
        }

        private void UpdateActivePoses()
        {
            if (IsReloading)
                return;

            if (IsChangingItem)
                return;

            currentHandsPose = null;
            currentAnimatedPose = null;

            if (heldItem == null)
                return;

            currentHandsPose = IsAiming ? heldItem.AimPose : heldItem.IdlePose;

            if (IsAttacking)
                return;

            if (fpsCharacterController != null && fpsCharacterController.IsRunningAndMoving())
                currentHandsPose = heldItem.RunPose;
        }

        private void UpdateMovementBounce(float deltaTime)
        {
            if (fpsCharacterController == null || heldItem == null || currentHandsPose == null)
                return;

            float sine = Mathf.Sin(Time.time * currentHandsPose.MovementBounceSpeed);
            float cos = Mathf.Cos(Time.time * 0.5f * currentHandsPose.MovementBounceSpeed);

            float moveVelocityMagnitude = fpsCharacterController.GetMoveVelocityMagnitude();
            moveVelocityMagnitude = Mathf.Min(moveVelocityMagnitude, heldItem.MovementBounceVelocityLimit);
            moveVelocityMagnitude = moveVelocityMagnitude / heldItem.MovementBounceVelocityLimit;

            movementBouncePositionOffset += moveVelocityMagnitude * new Vector2(
                deltaTime * ((0.5f - cos) * 2f) * currentHandsPose.MovementBounceStrength_Horizontal,
                deltaTime * sine * currentHandsPose.MovementBounceStrength_Vertical);

            Vector2 dampingForce =
                (heldItem.MovementBounceSpringStiffness * -movementBouncePositionOffset) - 
                (heldItem.MovementBounceSpringDamping * deltaTime * movementBouncePositionOffset);

            movementBouncePositionOffset += deltaTime * dampingForce;
        }

        private void UpdateHandsPosition()
        {
            if (handsTransform == null)
            {
                Debug.LogWarning($"{GetType().Name}.updateHandsPosition(): handsTransform == null - exiting early", gameObject);
                return;
            }

            if (handsParentTransform == null)
            {
                Debug.LogWarning($"{GetType().Name}.updateHandsPosition(): handsParentTransform == null - exiting early", gameObject);
                return;
            }

            if (currentHandsPose != null)
            {
                Vector3 targetHandsOffset = currentHandsPose.PositionOffset;
                Vector3 targetHandsEulerOffset = currentHandsPose.EulerOffset;

                if (IsChangingItem)
                {
                    if (equipPoseBlend > 0)
                    {
                        targetHandsOffset = Vector3.Lerp(targetHandsOffset, heldItem.EquipPose.PositionOffset, equipPoseBlend);
                        targetHandsEulerOffset = Vector3.Lerp(targetHandsEulerOffset, heldItem.EquipPose.EulerOffset, equipPoseBlend);
                    }
                    else if (unequipPoseBlend > 0)
                    {
                        targetHandsOffset = Vector3.Lerp(targetHandsOffset, heldItem.UnequipPose.PositionOffset, unequipPoseBlend);
                        targetHandsEulerOffset = Vector3.Lerp(targetHandsEulerOffset, heldItem.UnequipPose.EulerOffset, unequipPoseBlend);
                    }
                }

                handsPositionOffset = Vector3.SmoothDamp(
                    current: handsPositionOffset,
                    target: targetHandsOffset,
                    currentVelocity: ref handsPositionVelocity,
                    smoothTime: currentHandsPose.TransformSmoothDampTime,
                    maxSpeed: float.MaxValue,
                    deltaTime: Time.deltaTime);

                handsEulerOffset = Vector3.SmoothDamp(
                    current: handsEulerOffset,
                    target: targetHandsEulerOffset,
                    currentVelocity: ref handsEulerVelocity,
                    smoothTime: currentHandsPose.TransformSmoothDampTime,
                    maxSpeed: float.MaxValue,
                    deltaTime: Time.deltaTime);
            }
            else
            {
                handsPositionOffset = Vector3.zero;
                handsEulerOffset = Vector3.zero;
            }

            Vector3 targetPosition = handsParentTransform.position + handsParentTransform.TransformVector(handsPositionOffset + (Vector3)movementBouncePositionOffset);
            Quaternion targetRotation = handsParentTransform.rotation * Quaternion.Euler(handsEulerOffset);

            handsTransform.SetPositionAndRotation(targetPosition, targetRotation);
        }

        private Transform GetHandsBoneTransform(string boneName)
        {
            handsTransform.GetComponentsInChildren(includeInactive: true, handsChildTransforms);

            Transform resultBone = null;

            for (int i = 0; i < handsChildTransforms.Count; i++)
            {
                if (handsChildTransforms[i].name == boneName)
                {
                    resultBone =  handsChildTransforms[i];
                    break;
                }
            }

            handsChildTransforms.Clear();

            return resultBone;
        }

        private void PlayHandsAnimation(string animationStateName, float blendTime)
        {
            if (handsAnimator == null)
                return;

            if (animationStateName == currentHandsAnimationState)
                return;

            currentHandsAnimationState = animationStateName;

            if (handsAnimator.HasState(layerIndex: 0, Animator.StringToHash(animationStateName)))
                handsAnimator.CrossFadeInFixedTime(animationStateName, fixedTransitionDuration: blendTime, layer: 0);
            else
                Debug.LogWarning($"{GetType().Name}.playHandsAnimation(): hands animator does not have state '{animationStateName}'");
        }

        private void PlayItemAnimation(string animationStateName, float blendTime)
        {
            if (heldItemAnimator == null)
                return;

            if (animationStateName == currentItemAnimationState)
                return;

            currentItemAnimationState = animationStateName;

            if (heldItemAnimator.HasState(layerIndex: 0, Animator.StringToHash(animationStateName)))
                heldItemAnimator.CrossFadeInFixedTime(animationStateName, fixedTransitionDuration: blendTime, layer: 0);
            else
                Debug.LogWarning($"{GetType().Name}.playItemAnimation(): item animator does not have state '{animationStateName}'");
        }

        private IEnumerator Coroutine_UpdateAttackAnimation(int attackAnimationIndex)
        {
            var attackAnimSettings = heldItem.AttackAnimations[attackAnimationIndex];

            triggeredAnimationEvents.Clear();

            PlayHandsAnimation(attackAnimSettings.HandsAnimatorAttackStateName, attackAnimSettings.AttackAnimationBlendTime);
            PlayItemAnimation(attackAnimSettings.ItemAnimatorAttackStateName, attackAnimSettings.AttackAnimationBlendTime);

            float timer = 0f;

            while (timer < attackAnimSettings.AttackAnimationLength)
            {
                if (heldItem == null)
                    break;

                timer += Time.deltaTime;
                float animationTime = timer / attackAnimSettings.AttackAnimationLength;

                handsAnimator.SetFloat(handsAnimatorTimeParameter, animationTime);

                if (heldItemAnimator != null)
                    heldItemAnimator.SetFloat(heldItem.ItemAnimatorTimeParameter, animationTime);

                for (int i = 0; i < attackAnimSettings.AnimationEvents.Count; i++)
                {
                    if (triggeredAnimationEvents.Contains(i))
                        continue;

                    var animationEvent = attackAnimSettings.AnimationEvents[i];

                    if (animationTime < animationEvent.EventPosition)
                        continue;

                    triggeredAnimationEvents.Add(i);
                    OnAnimationEvent?.Invoke(animationEvent.EventMessage);
                }

                yield return null;
            }

            currentAttackAnimationIndex = 0;

            attackCoroutine = null;
        }

        public void JumpToFirstAnimationEventInPose(string eventName, bool skipPassedAnimationEvents = true, bool resetFutureAnimationEvents = true)
        {
            GetAnimatedPoseAnimationEventsByName(eventName, animationEventsList);

            // no matching animation events found - exiting early
            if (animationEventsList.Count == 0)
                return;

            float earliestTime = 1.0f;

            // search first looping animation event and jump to that animation time
            for (int i = 0; i < animationEventsList.Count; i++)
            {
                var animEvent = animationEventsList[i];

                if (animEvent.EventPosition < earliestTime)
                    earliestTime = animEvent.EventPosition;
            }

            JumpToPoseAnimationTimeNormalized(earliestTime, skipPassedAnimationEvents, resetFutureAnimationEvents);
        }

        public void JumpToLastAnimationEventInPose(string eventName, bool skipPassedAnimationEvents = true, bool resetFutureAnimationEvents = true)
        {
            GetAnimatedPoseAnimationEventsByName(eventName, animationEventsList);

            // no matching animation events found - exiting early
            if (animationEventsList.Count == 0)
                return;

            float latestTime = 0.0f;

            // search last reload loop end animation event and jump to that animation time
            for (int i = 0; i < animationEventsList.Count; i++)
            {
                var animEvent = animationEventsList[i];

                if (animEvent.EventPosition > latestTime)
                    latestTime = animEvent.EventPosition;
            }

            JumpToPoseAnimationTimeNormalized(latestTime, skipPassedAnimationEvents, resetFutureAnimationEvents);
        }

        public void JumpToPoseAnimationTimeNormalized(float poseTimeNormalized, bool skipPassedAnimationEvents = true, bool resetFutureAnimationEvents = true)
        {
            if (currentAnimatedPose == null)
                return;

            poseAnimationTimer = poseTimeNormalized * currentAnimatedPose.AnimationLength;

            handsAnimator.SetFloat(handsAnimatorTimeParameter, poseTimeNormalized);

            if (heldItemAnimator != null)
                heldItemAnimator.SetFloat(heldItem.ItemAnimatorTimeParameter, poseTimeNormalized);

            if (skipPassedAnimationEvents)
            {
                for (int i = 0; i < currentAnimatedPose.AnimationEvents.Count; i++)
                {
                    if (triggeredAnimationEvents.Contains(i))
                        continue;

                    var animationEvent = currentAnimatedPose.AnimationEvents[i];

                    if (poseTimeNormalized < animationEvent.EventPosition)
                        continue;

                    if (triggeredAnimationEvents.Contains(i) == false)
                        triggeredAnimationEvents.Add(i);
                }
            }

            if (resetFutureAnimationEvents)
            {
                if (skipPassedAnimationEvents)
                {
                    for (int i = 0; i < currentAnimatedPose.AnimationEvents.Count; i++)
                    {
                        if (triggeredAnimationEvents.Contains(i) == false)
                            continue;

                        var animationEvent = currentAnimatedPose.AnimationEvents[i];

                        if (poseTimeNormalized >= animationEvent.EventPosition)
                            continue;

                        if (triggeredAnimationEvents.Contains(i))
                            triggeredAnimationEvents.Remove(i);
                    }
                }
            }
        }
        
        public void GetAnimatedPoseAnimationEventsByName(string eventMessage, List<FPSItem.AnimationEvent> listToPopulate)
        {
            listToPopulate.Clear();

            if (currentAnimatedPose == null)
                return;

            for (int i = 0; i < currentAnimatedPose.AnimationEvents.Count; i++)
            {
                var animEvent = currentAnimatedPose.AnimationEvents[i];

                if (animEvent.EventMessage == eventMessage)
                    listToPopulate.Add(animEvent);
            }
        }

        private IEnumerator Coroutine_UpdateAnimatedPose(FPSItem.AnimatedItemPose animatedPose)
        {
            currentHandsPose = animatedPose;
            currentAnimatedPose = animatedPose;

            triggeredAnimationEvents.Clear();

            PlayHandsAnimation(animatedPose.HandsAnimationStateName, animatedPose.AnimationStateBlendTime);
            PlayItemAnimation(animatedPose.ItemAnimationStateName, animatedPose.AnimationStateBlendTime);

            poseAnimationTimer = 0f;

            while (poseAnimationTimer < animatedPose.AnimationLength)
            {
                if (heldItem == null)
                    break;

                UpdateAnimatedPoseAnimationEvents();
                yield return null;
                poseAnimationTimer += Time.deltaTime;
            }

            reloadCoroutine = null;
        }

        public void SetHeldItem(FPSItem item, bool skipAnimation)
        {
            if (item == heldItem)
                return;

            StopActiveCoroutines(allowWeaponChangeRoutine: false);

            if (skipAnimation)
                heldItem = item;
            else
                changeItemCoroutine = StartCoroutine(Coroutine_ChangeHeldItem(item));
        }

        private IEnumerator Coroutine_ChangeHeldItem(FPSItem newItem)
        {
            equipPoseBlend = 0f;
            unequipPoseBlend = 0f;

            if (heldItem != null)
            {
                currentHandsPose = heldItem.IdlePose;
                currentAnimatedPose = heldItem.UnequipPose;
                triggeredAnimationEvents.Clear();

                PlayHandsAnimation(currentAnimatedPose.HandsAnimationStateName, currentAnimatedPose.AnimationStateBlendTime);
                PlayItemAnimation(currentAnimatedPose.ItemAnimationStateName, currentAnimatedPose.AnimationStateBlendTime);

                poseAnimationTimer = 0f;

                while (poseAnimationTimer < currentAnimatedPose.AnimationLength)
                {
                    if (heldItem == null)
                        break;

                    UpdateAnimatedPoseAnimationEvents();
                    yield return null;
                    poseAnimationTimer += Time.deltaTime;
                    unequipPoseBlend = poseAnimationTimer / currentAnimatedPose.AnimationLength;
                }
            }

            heldItem = newItem;
            unequipPoseBlend = 1f;

            if (heldItem != null)
            {
                currentHandsPose = heldItem.IdlePose;
                currentAnimatedPose = heldItem.EquipPose;
                triggeredAnimationEvents.Clear();

                yield return null;

                PlayHandsAnimation(currentAnimatedPose.HandsAnimationStateName, currentAnimatedPose.AnimationStateBlendTime);
                PlayItemAnimation(currentAnimatedPose.ItemAnimationStateName, currentAnimatedPose.AnimationStateBlendTime);

                poseAnimationTimer = 0f;

                while (poseAnimationTimer < currentAnimatedPose.AnimationLength)
                {
                    if (heldItem == null)
                        break;

                    UpdateAnimatedPoseAnimationEvents();
                    yield return null;
                    poseAnimationTimer += Time.deltaTime;
                    equipPoseBlend = 1.0f - (poseAnimationTimer / currentAnimatedPose.AnimationLength);
                }
            }

            equipPoseBlend = 0f;
            unequipPoseBlend = 0f;
            changeItemCoroutine = null;
        }

        private void UpdateAnimatedPoseAnimationEvents()
        {
            float animationTime = poseAnimationTimer / currentAnimatedPose.AnimationLength;

            handsAnimator.SetFloat(handsAnimatorTimeParameter, animationTime);

            if (heldItemAnimator != null)
                heldItemAnimator.SetFloat(heldItem.ItemAnimatorTimeParameter, animationTime);

            for (int i = 0; i < currentAnimatedPose.AnimationEvents.Count; i++)
            {
                if (triggeredAnimationEvents.Contains(i))
                    continue;

                var animationEvent = currentAnimatedPose.AnimationEvents[i];

                if (animationTime < animationEvent.EventPosition)
                    continue;

                triggeredAnimationEvents.Add(i);
                OnAnimationEvent?.Invoke(animationEvent.EventMessage);
            }
        }

        public void DebugLogAnimationEvent(string animationEvent)
        {
            Debug.Log($"{GetType().Name}.DebugLogAnimationEvent(): {animationEvent}");
        }
    }
}