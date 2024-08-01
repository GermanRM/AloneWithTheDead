using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{

    [Header("Script References")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerSneak sneak;

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
        
    }

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
