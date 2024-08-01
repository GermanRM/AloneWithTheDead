using System;
using System.Collections;
using System.Collections.Generic;
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
    public event Action OnZombieDamageTaken;

    public event Action OnZombieDead;

    #endregion

    // Update is called once per frame
    void Update()
    {
        AttackPlayer();

        if (zombieHealth <= 0)
        {
            isAlive = false;
        }
    }

    #region Attack

    private void AttackPlayer()
    {
        if (zombieMovement.CanAttack(canAttack))
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

    public void DamageZombie(float damage)
    {
        zombieHealth -= damage;
        OnZombieDamageTaken?.Invoke();

        if (zombieHealth <= 0)
        {
            OnZombieDead?.Invoke();
        }
    }

    #endregion
}
