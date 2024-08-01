using System;
using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Health Properties")]
    [SerializeField] private float playerHealth;
    public bool isDeath;

    [Header("Player Combat Properties")]
    [SerializeField] private float actualDamage;
    [SerializeField] private bool canShoot;
    [SerializeField] private bool canSwitchWeapon = true;
    [SerializeField] private float weaponSwitchDelay;
    [Space]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask defaultLayer;

    [Header("Player Aim Properties")]
    public bool isAiming;

    [Header("Ammo Properties")]
    [SerializeField] private int ammoInCartridge;
    [SerializeField] private int totalAmmo;

    [Header("Player Inventory Properties")]
    [SerializeField] private FPSItem actualItem;
    [SerializeField] private List<FPSItem> items;

    [Header("Gun References")]
    [SerializeField] private FPSItem knifeItem;
    [SerializeField] private FPSItem pistolItem;
    [SerializeField] private FPSItem flashlightItem;

    [Header("Script References")]
    [SerializeField] private HandsController handsController;
    [SerializeField] private PlayerMovement playerMovement;

    #region Events 

    public event Action<float> OnPlayerDamaged;
    public event Action<float> OnPlayerHealed;
    public event Action OnPlayerDeath;

    public event Action<FPSItem> OnPlayerAttack;
    public event Action<FPSItem> OnPlayerAttackAnimStart;

    public event Action<FPSItem> OnPlayerItemSwitch;

    public event Action<FPSItem> OnPlayerStartReload;
    public event Action<FPSItem> OnPlayerFinishReload;

    public event Action OnPlayerStartAiming;
    public event Action OnPlayerEndAiming;

    #endregion

    private PlayerControls playerControls;

    // Start is called before the first frame update
    void Start()
    {
        playerControls = new PlayerControls();
        playerControls.Inventory.Enable();
        playerControls.Interactions.Enable();

        handsController.OnAnimationEvent.AddListener(OnHandAnimationEvent);
    }

    private void OnDisable()
    {
        handsController.OnAnimationEvent.RemoveListener(OnHandAnimationEvent);
    }

    // Update is called once per frame
    void Update()
    {
        WeaponSwitcher();
        Shoot();
        Reload();
    }

    #region Player Health

    public void DamagePlayer(float damage)
    {
        playerHealth -= damage;
        OnPlayerDamaged?.Invoke(damage);

        if (playerHealth <= 0)
        {
            isDeath = true;
            OnPlayerDeath?.Invoke();
        }

    }

    public void HealPlayer(float heal)
    {
        playerHealth += heal;
        OnPlayerHealed?.Invoke(heal);
    }

    #endregion

    #region Shoot

    private void Shoot()
    {
        if (playerControls.Interactions.Aim.WasPerformedThisFrame() && !playerMovement.IsRunningAndMoving())
        {
            isAiming = true;
            OnPlayerStartAiming?.Invoke();
        }
        if (playerControls.Interactions.Aim.WasReleasedThisFrame() && isAiming)
        {
            isAiming = false;
            OnPlayerEndAiming?.Invoke();    
        }

        if (playerControls.Interactions.Attack.WasPerformedThisFrame())
        {
            if (actualItem.weaponType == FPSItem.WeaponType.Melee)
            {
                if (CanShoot())
                {
                    handsController.ApplyAttackAnim(); //Do the animation
                    StartCoroutine(CanShootDelay(actualItem.WeaponAttackDelay)); // Start the coroutine to enable CanShoot again
                    OnPlayerAttack?.Invoke(actualItem); //Invoke event

                    //Do the boxcast or spherecast thing
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, actualItem.WeaponReach, (defaultLayer | enemyLayer)))
                    {
                        //Do the damage, etc

                        Debug.Log(hit.collider.gameObject.name);
                        ZombieHitbox hitbox = hit.collider.gameObject.GetComponent<ZombieHitbox>();

                        if (hitbox != null) { hitbox.ApplyDamageInRegion(actualDamage, actualItem); }
                    }
                }
            }

            if (actualItem.weaponType == FPSItem.WeaponType.Fire)
            {
                if (CanShoot())
                {
                    handsController.ApplyAttackAnim(); //Do the animation
                    StartCoroutine(CanShootDelay(actualItem.WeaponAttackDelay)); // Start the coroutine to enable CanShoot again
                    OnPlayerAttack?.Invoke(actualItem); //Invoke event

                    //Do the raycast thing
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, (defaultLayer | enemyLayer)))
                    {
                        //do the damage, etc
                        Debug.Log(hit.collider.gameObject.name);

                        ZombieHitbox hitbox = hit.collider.gameObject.GetComponent<ZombieHitbox>();

                        if (hitbox != null) { hitbox.ApplyDamageInRegion(actualDamage, actualItem); }
                    }
                }
            }

            if (actualItem.weaponType == FPSItem.WeaponType.Utilitie)
            {

            }
        }
    }

    private IEnumerator CanShootDelay(float seconds)
    {
        canShoot = false;
        yield return new WaitForSeconds(seconds);
        canShoot = true;
    }

    #endregion

    #region Reload

    private void Reload()
    {
        if (playerControls.Interactions.Reload.WasPerformedThisFrame())
        {
            if (actualItem.weaponType == FPSItem.WeaponType.Fire && CanReload())
            {
                handsController.ApplyReloadAnim(); //Apply reload anim
                OnPlayerStartReload?.Invoke(actualItem); //Invoke event
            }
        }
    }

    #endregion

    #region Inventory

    private void WeaponSwitcher()
    {
        if (playerControls.Inventory.Knife.WasPerformedThisFrame() && CanChangeItem())
        {
            if (items.Contains(knifeItem) && actualItem != knifeItem)
            {
                actualItem = knifeItem;
                handsController.SetHeldItem(knifeItem, false);
                OnPlayerItemSwitch?.Invoke(actualItem);
                StartCoroutine(SwitchWeaponDelay(weaponSwitchDelay));
            }          
        }

        if (playerControls.Inventory.Pistol.WasPerformedThisFrame() && CanChangeItem())
        {
            if (items.Contains(pistolItem) && actualItem != pistolItem) 
            {
                actualItem = pistolItem;
                handsController.SetHeldItem(pistolItem, false);
                OnPlayerItemSwitch?.Invoke(actualItem);
                StartCoroutine(SwitchWeaponDelay(weaponSwitchDelay));
            } 
        }

        if (playerControls.Inventory.Flashlight.WasPerformedThisFrame() && CanChangeItem())
        {
            if (items.Contains(flashlightItem) && actualItem != flashlightItem)
            {
                actualItem = flashlightItem;
                handsController.SetHeldItem(flashlightItem, false);
                OnPlayerItemSwitch?.Invoke(actualItem);
                StartCoroutine(SwitchWeaponDelay(weaponSwitchDelay));
            }
        }
    }

    private IEnumerator SwitchWeaponDelay(float seconds)
    {
        canSwitchWeapon = false;
        yield return new WaitForSeconds(seconds);
        canSwitchWeapon = true;
    }

    /// <summary>
    /// Add this item to the inventory
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(FPSItem item)
    {
        items.Add(item);
    }

    #endregion

    private void OnHandAnimationEvent(string name)
    {
        if (name == "CheckForDamage") OnPlayerAttackAnimStart?.Invoke(actualItem);
        if (name == "ReloadAll") OnPlayerFinishReload?.Invoke(actualItem);
    }

    #region Checkers

    public bool CanChangeItem()
    {
        if (!canSwitchWeapon) return false;
        if (!handsController.CanChangeItem()) return false;

        return true;
    }

    public bool CanShoot()
    {
        if (!canShoot)
            return false;

        if (actualItem == null)
            return false;

        if (actualItem.AttackAnimations.Count == 0) // if the actual item dont have attack animations return false
            return false;

        if (handsController.IsReloading) // if reloading animations is not finished
            return actualItem.CanCancelReloadAnimationByShooting; // if can cancel reload anim by shooting return true, otherwise return false

        if (handsController.IsChangingItem) // if item switch animation is not finished
            return false;

        return true;
    }

    public bool CanReload()
    {
        if (actualItem == null)
            return false;

        if (handsController.IsReloading)
            return false;

        if (handsController.IsChangingItem)
            return false;

        return true;
    }

    #endregion

    #region Debug

    #endregion
}
