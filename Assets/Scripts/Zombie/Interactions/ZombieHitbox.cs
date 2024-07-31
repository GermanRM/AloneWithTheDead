using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHitbox : MonoBehaviour
{
    [Header("Hitbox Properties")]
    [SerializeField] private HitboxRegion hitboxRegion;
    [SerializeField] private float damageMultiplier;
    [SerializeField] private ZombieCombat zombieCombat;

    public void ApplyDamageInRegion(float damage)
    {
        zombieCombat.DamageZombie(damage * damageMultiplier);
    }
}
