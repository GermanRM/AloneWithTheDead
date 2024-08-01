using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour
{
    public Animator animator;
    public GameObject doorControlPanel; // El panel de control de la puerta
    public TMP_Text doorStatusText; // El texto del estado de la puerta
    public Collider detectedCollider; // El colisionador del objeto Detected
    public GameObject newProximityCollider; // El nuevo colisionador para detección de proximidad

    private bool isOpen = false; // Estado inicial de la puerta
    private bool playerNearby = false; // Estado de proximidad del jugador

    void Start()
    {
        doorControlPanel.SetActive(false); // Panel de control desactivado inicialmente
    }

    void Update()
    {
        if (playerNearby)
        {
            if (Input.GetKeyDown(KeyCode.N) && !isOpen)
            {
                OpenDoor();
            }
            else if (Input.GetKeyDown(KeyCode.M) && isOpen)
            {
                CloseDoor();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (detectedCollider.enabled)
            {
                if (animator != null)
                {
                    animator.SetBool("hasKey", true);
                }
                detectedCollider.enabled = false; // Desactiva el colisionador del objeto Detected

                if (newProximityCollider != null)
                {
                    newProximityCollider.SetActive(true); // Activa el nuevo colisionador de proximidad
                }
            }
            else
            {
                playerNearby = true;
                doorControlPanel.SetActive(true); // Muestra el panel de control
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            doorControlPanel.SetActive(false); // Oculta el panel de control cuando el jugador se aleja
        }
    }

    public void OpenDoor()
    {
        if (animator != null)
        {
            animator.SetBool("isOpen", true);
        }
        isOpen = true;
        UpdateDoorStatusText();
    }

    public void CloseDoor()
    {
        if (animator != null)
        {
            animator.SetBool("isOpen", false);
        }
        isOpen = false;
        UpdateDoorStatusText();
    }

    private void UpdateDoorStatusText()
    {
        if (doorStatusText != null)
        {
            doorStatusText.text = isOpen ? "Cerrar" : "Abrir";
        }
    }
}