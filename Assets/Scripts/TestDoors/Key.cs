using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    public GameObject ObjectKey; // Este será el cilindro llamado "key"
    public GameObject ColliderDoor; // Este será el detector de la puerta
    public GameObject newProximityCollider; // El nuevo colisionador para detección de proximidad

    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            if (ColliderDoor != null)
            {
                ColliderDoor.SetActive(true); // Activa el detector de la puerta
            }

            if (ObjectKey != null)
            {
                ObjectKey.SetActive(false); // Desactiva la llave
            }
        }
    }
}