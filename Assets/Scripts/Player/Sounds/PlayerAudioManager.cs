using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [Header("Audio Properties")]
    [SerializeField] private AudioSource source;

    [Header("Movement")]
    [SerializeField] private List<AudioClip> footstepsSounds = new List<AudioClip>();
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip crouchSound;
    [SerializeField] private AudioClip getUpSound;

    [Header("Combat")]
    [SerializeField] private List<AudioClip> playerHurtSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> playerHealSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> playerAttackMeleeSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> playerAttackFireSounds = new List<AudioClip>();

    [Header("Inventory")]
    [SerializeField] private AudioClip playerSwitchMeleeSound;
    [SerializeField] private AudioClip playerSwitchFireSound;
    [SerializeField] private AudioClip playerStartReloadSound;
    [SerializeField] private AudioClip playerFinishReloadSound;
    [SerializeField] private AudioClip playerStartAimingSound;
    [SerializeField] private AudioClip playerFinishAimingSound;

    [Header("Script References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerMovement playerMovement;

    private void Update()
    {
        this.enabled = !playerStats.isDeath;
    }

    private void OnEnable()
    {
        playerMovement.OnPlayerMove += OnPlayerMovement;
        playerMovement.OnPlayerJump += OnPlayerJumped;
        playerMovement.OnPlayerLand += OnPlayerLanded;
        playerMovement.OnPlayerCrouch += OnPlayerCrouched;
        playerMovement.OnPlayerGetUp += OnPlayerGetuped;

        playerStats.OnPlayerAttack += OnPlayerAttacks;
        playerStats.OnPlayerDamaged += OnPlayerHurted;
        playerStats.OnPlayerHealed += OnPlayerGetHealed;

        playerStats.OnPlayerItemSwitch += OnItemSwitch;
        playerStats.OnPlayerStartReload += OnStartReload;
        playerStats.OnPlayerFinishReload += OnFinishReload;
        playerStats.OnPlayerStartAiming += OnStartAim;
        playerStats.OnPlayerEndAiming += OnEndAim;
    }

    private void OnDisable()
    {
        playerMovement.OnPlayerMove -= OnPlayerMovement;
        playerMovement.OnPlayerJump -= OnPlayerJumped;
        playerMovement.OnPlayerLand -= OnPlayerLanded;
        playerMovement.OnPlayerCrouch -= OnPlayerCrouched;
        playerMovement.OnPlayerGetUp -= OnPlayerGetuped;

        playerStats.OnPlayerAttack -= OnPlayerAttacks;
        playerStats.OnPlayerDamaged -= OnPlayerHurted;
        playerStats.OnPlayerHealed -= OnPlayerGetHealed;

        playerStats.OnPlayerItemSwitch -= OnItemSwitch;
        playerStats.OnPlayerStartReload -= OnStartReload;
        playerStats.OnPlayerFinishReload -= OnFinishReload;
        playerStats.OnPlayerStartAiming -= OnStartAim;
        playerStats.OnPlayerEndAiming -= OnEndAim;
    }

    private void OnPlayerMovement() 
    {
        if (!source.isPlaying)
            source.PlayOneShot(footstepsSounds[Random.Range(0, footstepsSounds.Count)]);
    }

    private void OnPlayerJumped() 
    {
            source.PlayOneShot(jumpSound);
    }

    private void OnPlayerLanded() 
    {
            source.PlayOneShot(landSound);
    }

    private void OnPlayerCrouched() 
    {
            source.PlayOneShot(crouchSound);
    }

    private void OnPlayerGetuped() 
    {
            source.PlayOneShot(getUpSound);
    }

    private void OnPlayerAttacks(FPSItem item) 
    {
        if (item.weaponType == FPSItem.WeaponType.Melee)
        {
                source.PlayOneShot(playerAttackMeleeSounds[Random.Range(0, playerAttackMeleeSounds.Count)]);
        }

        if (item.weaponType == FPSItem.WeaponType.Fire)
        {
                source.PlayOneShot(playerAttackFireSounds[Random.Range(0, playerAttackFireSounds.Count)]);
        }
    }

    private void OnPlayerHurted(float damage) 
    {
            source.PlayOneShot(playerHurtSounds[Random.Range(0, playerHurtSounds.Count)]);
    }

    private void OnPlayerGetHealed(float heal) { }

    private void OnItemSwitch(FPSItem item) 
    {
        if (item.weaponType == FPSItem.WeaponType.Melee)
        {
                source.PlayOneShot(playerSwitchMeleeSound);
        }

        if (item.weaponType == FPSItem.WeaponType.Fire)
        {
                source.PlayOneShot(playerSwitchFireSound);
        }
    }

    private void OnStartReload(FPSItem item) 
    {
        source.PlayOneShot(playerStartReloadSound);
    }

    private void OnFinishReload(FPSItem item) 
    {
        source.PlayOneShot(playerFinishReloadSound);
    }

    private void OnStartAim() 
    {
        //if (!source.isPlaying)
            //source.PlayOneShot(playerStartAimingSound);
    }

    private void OnEndAim() 
    {
        //if (!source.isPlaying)
            //source.PlayOneShot(playerFinishReloadSound);
    }

}
