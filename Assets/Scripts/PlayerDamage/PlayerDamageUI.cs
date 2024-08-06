using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup damagePanelCanvasGroup;
    [SerializeField] private PlayerStats playerStats;

    private void OnEnable()
    {
        playerStats.OnPlayerDamaged += UpdateDamagePanelAlpha;
        playerStats.OnPlayerHealed += UpdateDamagePanelAlpha;
    }

    private void OnDisable()
    {
        playerStats.OnPlayerDamaged -= UpdateDamagePanelAlpha;
        playerStats.OnPlayerHealed -= UpdateDamagePanelAlpha;
    }

    private void UpdateDamagePanelAlpha(float _)
    {
        float maxHealth = 20f; // Salud máxima del jugador
        float currentHealth = playerStats.playerHealth; // Obtener la salud actual del jugador
        Debug.Log($"Current Health: {currentHealth}");
        float alpha = 1 - (currentHealth / maxHealth);
        Debug.Log($"Alpha: {alpha}");
        damagePanelCanvasGroup.alpha = alpha;
    }

    private void Start()
    {
        UpdateDamagePanelAlpha(0); // Inicializar el alfa del panel
    }
}