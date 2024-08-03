using System;
using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Pickup Properties")]
    [SerializeField] private float pickupDistance;
    [SerializeField] private LayerMask interacteableMask;
    GameObject lastHitObject;
    private Outline currentOutline;

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
    }

    private void OnDisable()
    {
        stats.OnPlayerAttack -= OnPlayerAttacks;
    }

    // Update is called once per frame
    void Update()
    {
        Pickup();
    }

    #region Interactions

    private void Pickup()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, pickupDistance, interacteableMask))
        {
            if (hit.collider.CompareTag("Interacteable"))
            {

                if (playerControls.Interactions.Interact.WasPerformedThisFrame())
                {
                    PickupItem pickupItem = hit.collider.GetComponent<PickupItem>();
                    stats.AddItem(pickupItem.fpsItem);
                    OnPlayerPickupItem?.Invoke();
                    pickupItem.DestroyObject();
                }

                #region Render outline

                GameObject hitObject = hit.collider.gameObject;

                // Solo hacer algo si el objeto al que apuntamos ha cambiado
                if (hitObject != lastHitObject)
                {
                    // Restablecer el material del último objeto interactuado
                    ResetLastHitObject();

                    // Guardar el nuevo objeto interactuado
                    lastHitObject = hitObject;

                    // Obtener el material del nuevo objeto
                    currentOutline = hitObject.GetComponent<Outline>();

                    // Cambiar la escala
                    currentOutline.enabled = true;

                    #endregion
                }
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

    #endregion

    #region Combat

    private void OnPlayerAttacks(FPSItem item)
    {
        if (item.weaponType == FPSItem.WeaponType.Fire) //Si es un arma de fuego
        {
            sneak.MakeSneakNoise(item.WeaponNoise); //Make Noise
        }

        if (item.weaponType == FPSItem.WeaponType.Melee) //Si es un arma cuerpo a cuerpo
        {
            sneak.MakeSneakNoise(item.WeaponNoise); //Make Noise
        }
    }

    #endregion
}
