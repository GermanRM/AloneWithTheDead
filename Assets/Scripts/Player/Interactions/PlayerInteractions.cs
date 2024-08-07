using System;
using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Pickup Properties")]
    [SerializeField] private float pickupDistance;
    [SerializeField] private LayerMask interacteableMask;

    [Header("Outline Properties")]
    [SerializeField] private string[] tagsToCheck;
    GameObject lastHitObject;
    private Outline currentOutline;

    [Header("Player Hurt Properties")]
    [SerializeField] private Image hurtImage;

    [Header("Shot Particles Properties")]
    [SerializeField] private Transform shotParticleParent;
    [SerializeField] private GameObject shotParticlePrefab;

    [Header("Script References")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerSneak sneak;
    private PlayerControls playerControls;

    #region Events

    public event Action OnPlayerPickupItem;

    #endregion

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerControls.Interactions.Enable();
    }

    private void OnEnable()
    {
        stats.OnPlayerAttack += OnPlayerAttacks;
        stats.OnPlayerDamaged += OnPlayerHurts;
    }

    private void OnDisable()
    {
        stats.OnPlayerAttack -= OnPlayerAttacks;
        stats.OnPlayerDamaged -= OnPlayerHurts;
    }

    // Update is called once per frame
    void Update()
    {
        Pickup();
        OutlineInteractuable();

        if (Input.GetKeyDown(KeyCode.P))
        {
            stats.DamagePlayer(1);
        }
    }

    #region Pickup

    private void Pickup()
    {
        if (playerControls.Interactions.Interact.WasPerformedThisFrame())
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, pickupDistance, interacteableMask))
            {
                if (hit.collider.CompareTag("Gun"))
                {
                    PickupItem pickupItem = hit.collider.GetComponent<PickupItem>();
                    stats.AddItem(pickupItem.fpsItem);
                    OnPlayerPickupItem?.Invoke();
                    pickupItem.DestroyObject();
                }

                if (hit.collider.CompareTag("Door"))
                {
                    DoorManager doorManager = hit.collider.GetComponentInParent<DoorManager>();
                    if (doorManager.isOpened == false)
                        doorManager.OpenDoor();
                    else
                        doorManager.CloseDoor();
                }
            }
        }
    }

    #endregion

    #region Outline Manager

    private void OutlineInteractuable()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, pickupDistance, interacteableMask))
        {
            if (IsTagInArray(hit.collider.tag, tagsToCheck))
            {
                lastHitObject = hit.collider.gameObject;

                // Obtener el material del nuevo objeto
                currentOutline = lastHitObject.GetComponent<Outline>();

                // Cambiar la escala
                currentOutline.enabled = true;
            }
            else
            {
                ResetLastHitObject();
            }
        }
        else
        {
            ResetLastHitObject();
        }
    }


    void ResetLastHitObject()
    {
        if (lastHitObject != null)
        {
            // Restablecer la escala del material al valor original
            currentOutline.enabled = false;

            // Limpiar el estado
            lastHitObject = null;
            currentOutline = null;
        }
    }

    /// <summary>
    /// We use this to check if the tag of the raycast is in the tagsList (tagsToCheck)
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    bool IsTagInArray(string tag, string[] tags)
    {
        foreach (string t in tags)
        {
            if (tag == t)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Combat

    private void OnPlayerAttacks(FPSItem item)
    {
        if (item.weaponType == FPSItem.WeaponType.Fire) //Si es un arma de fuego
        {
            Instantiate(shotParticlePrefab, shotParticleParent);
            sneak.MakeSneakNoise(item.WeaponNoise); //Make Noise
        }

        if (item.weaponType == FPSItem.WeaponType.Melee) //Si es un arma cuerpo a cuerpo
        {
            sneak.MakeSneakNoise(item.WeaponNoise); //Make Noise
        }
    }

    private void OnPlayerHurts(float damage)
    {
        switch (stats.playerHealth)
        {
            case 4:
                hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, 0);
                break;
            case 3:
                hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, 0.3f);
                break;
            case 2:
                hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, 0.6f);
                break;
            case 1:
                hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, 0.8f);
                break;
            case 0:
                hurtImage.color = new Color(hurtImage.color.r, hurtImage.color.g, hurtImage.color.b, 1);
                break;
        }
    }

    #endregion
}
