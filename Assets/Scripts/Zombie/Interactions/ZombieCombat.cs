using System;
using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class ZombieCombat : MonoBehaviour
{
    [Header("Zombie Health Properties")]
    [SerializeField] private float zombieHealth;
    public bool isAlive;

    [Header("Zombie Combat Properties")]
    [SerializeField] private float damage;
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float attackDelay;
    private float attackCounterDelay;

    [Header("Script References")]
    [SerializeField] private ZombieMovement zombieMovement;

    #region Events

    public event Action OnZombieAttack;
    public event Action<FPSItem> OnZombieDamageTaken;

    public event Action OnZombieDead;

    #endregion

    // Update is called once per frame
    void Update()
    {
        AttackPlayer();
    }

    #region Attack

    private void AttackPlayer()
    {
        if (zombieMovement.CanAttack(canAttack) && isAlive)
        {
            StartCoroutine(attackDelayCoroutine());

            if (zombieMovement.GetTargetTransform().GetComponent<PlayerStats>() == null)
            {
                OnZombieAttack?.Invoke();
                PlayerStats stats = zombieMovement.GetTargetTransform().GetComponentInParent<PlayerStats>();
                stats.DamagePlayer(damage);
            }
            else
            {
                OnZombieAttack?.Invoke();
                PlayerStats stats = zombieMovement.GetTargetTransform().GetComponent<PlayerStats>();
                stats.DamagePlayer(damage);
            }
        }
    }

    private IEnumerator attackDelayCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }

    #endregion

    #region Health

    public void DamageZombie(float damage, FPSItem item)
    {
        zombieHealth -= damage;
        OnZombieDamageTaken?.Invoke(item);

        if (zombieHealth <= 0)
        {
            isAlive = false;
            OnZombieDead?.Invoke();
            GameManager.Instance.AddZombieKilled(1);
        }

    }

    #endregion
}
