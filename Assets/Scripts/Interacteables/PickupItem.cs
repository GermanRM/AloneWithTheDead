using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Properties (Gun)")]
    public FPSItem fpsItem;

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

}
