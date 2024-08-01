using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class ZombieHitbox : MonoBehaviour
{
    [Header("Hitbox Properties")]
    [SerializeField] private HitboxRegion hitboxRegion;
    [SerializeField] private float damageMultiplier;
    [SerializeField] private ZombieCombat zombieCombat;

    public void ApplyDamageInRegion(float damage, FPSItem item)
    {
        zombieCombat.DamageZombie(damage * damageMultiplier, item);
    }
}
