using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Tensori.FPSHandsHorrorPack
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Tensori/FPSHorrorPack/New FPS Item Asset")]
    public class FPSItem : ScriptableObject
    {
        public enum WeaponType
        {
            Melee,
            Utilitie,
            Fire
        }

        [Header("Weapon Stats")]
        [Tooltip("Damage of the weapon")]
        [Min(1)]public float WeaponDamage;

        [Tooltip("Weapon Noise when attack")]
        [Min(1)] public float WeaponNoise;

        [Tooltip("Type of the weapon")]
        public WeaponType weaponType;

        [Tooltip("Reach of the weapon (if is melee)")]
        [Min(1)] public float WeaponReach;

        [Tooltip("Attack delay of the weapon")]
        public float WeaponAttackDelay;

        [Tooltip("Max Ammo in the cartridge")]
        [Min(1)] public int AmmunitionClipSize = 8;

        [Space]

        [Tooltip("This item object is instantiated as a child object to the desired pivot bone inside the active FPS hands' transform hierarchy.\nPlease set the name of desired parent bone to the 'Hands Pivot Bone Transform Name' parameter.")]
        public GameObject ItemPrefab = null;

        [Header("Gameplay Settings")]
        public bool CanCancelReloadAnimationByShooting = false;
        public bool CanCancelReloadAnimationByChangingItem = true;
        [Space]
        public bool EnableAmmunitionUI = false;

        [Header("Animator Settings")]
        [Tooltip("Name of the target parent transform inside hands game object transform hierarchy")]
        public string HandsPivotBoneTransformName = "WeaponPivot";

        [Tooltip("Name of the animator parameter that is used to control the playback position of animation states from the item's animator component.")]
        public string ItemAnimatorTimeParameter = "AnimationTime";
  
        [Space]

        public List<AttackAnimationSettings> AttackAnimations = new List<AttackAnimationSettings>();

        [Header("General Movement Bounce Settings")]
        [Tooltip("Maximum magnitude of the player's movement velocity that is used to calculate the strength of procedural movement bounce animation. If the player's movement velocity magnitude is over this limit, the temporary velocity magnitude variable is clamped to this value.")]
        [Min(0f)] public float MovementBounceVelocityLimit = 3.0f;

        [Tooltip("The strength or sharpness of the virtual spring used to calculate procedural movement bounce animation.")]
        [Min(0f)] public float MovementBounceSpringStiffness = 250.0f;

        [Tooltip("The stopping force or 'smoothness' of the virtual spring used to calculate procedural movement bounce animation.")]
        [Min(0f)] public float MovementBounceSpringDamping = 30.0f;

        [Header("Poses")]
        public ItemPose IdlePose = new ItemPose();
        public ItemPose RunPose = new ItemPose();
        public ItemPose AimPose = new AnimatedItemPose();
        public AnimatedItemPose ReloadPose = new AnimatedItemPose();
        public AnimatedItemPose EquipPose = new AnimatedItemPose();
        public AnimatedItemPose UnequipPose = new AnimatedItemPose();

        [System.Serializable]
        public class AttackAnimationSettings
        {
            [Tooltip("Name of the attack / shoot animation state in the animator of FPS hands")]
            public string HandsAnimatorAttackStateName;

            [Tooltip("Name of the attack / shoot animation state in the animator of item")]
            public string ItemAnimatorAttackStateName;

            [Tooltip("Controls the length of this animation.\n1 unit = 1 second.")]
            [Min(0f)] public float AttackAnimationLength = 1.0f;

            [Tooltip("Controls the length of animator crossfade into this animation.\n1 unit = 1 second.")]
            [Min(0f)] public float AttackAnimationBlendTime = 0f;

            public List<AnimationEvent> AnimationEvents = new List<AnimationEvent>();
        }

        [System.Serializable]
        public class AnimationEvent
        {
            [Tooltip("Message text that is used to identify the event")]
            public string EventMessage = "Animation Event";

            [Tooltip("Normalized time on the animation's timeline when the event gets triggered.\n0.0f = start of animation, 1.0f = end of animation")]
            [Range(0f, 1f)] public float EventPosition = 0.5f;
        }

        [System.Serializable]
        public class ItemPose
        {
            [Header("Animator Settings")]
            [Tooltip("Animation state inside the hands' animator")]
            public string HandsAnimationStateName;

            [Tooltip("Animation state inside item's animator")]
            public string ItemAnimationStateName;

            [Tooltip("Controls the length of animator crossfade into animations when this pose is active.\n1 unit = 1 second.")]
            [Min(0f)] public float AnimationStateBlendTime = 0.1f;

            [Header("Transform Settings")]
            [Tooltip("Local position offset of hands")]
            public Vector3 PositionOffset;

            [Tooltip("Local euler rotation offset of hands")]
            public Vector3 EulerOffset;

            [Tooltip("Blend time when transitioning into this pose.\n1 unit = 1 second.")]
            public float TransformSmoothDampTime = 0.1f;

            [Header("Pose-Specific Movement Bounce Settings")]
            [Tooltip("Horizontal position range of the movement bounce motion in this pose.")]
            public float MovementBounceStrength_Horizontal = 1.0f;

            [Tooltip("Vertical position range of the movement bounce motion in this pose.")]
            public float MovementBounceStrength_Vertical = 1.0f;

            [Tooltip("Speed of the movement bounce motion in this pose.")]
            [Min(0f)] public float MovementBounceSpeed = 12f;
        }

        [System.Serializable]
        public class AnimatedItemPose : ItemPose
        {
            [Header("Additional Animation Settings")]
            [Tooltip("Controls the length of animation playback.\n1 unit = 1 second.")]
            [Min(0f)] public float AnimationLength = 1.0f;

            public List<AnimationEvent> AnimationEvents = new List<AnimationEvent>();
        }
    }
}