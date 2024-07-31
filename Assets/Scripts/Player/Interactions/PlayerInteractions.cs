using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{

    [Header("Script References")]
    [SerializeField] private PlayerStats stats;

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

    }

    #endregion
}
