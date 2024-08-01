using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class ZombieAnimation : MonoBehaviour
{


    [Header("Script References")]
    [SerializeField] private Animator animator;
    [SerializeField] private ZombieStateMachine stateMachine;
    [SerializeField] private ZombieCombat combat;
    [SerializeField] private ZombieMovement movement;

    private void OnEnable()
    {
        combat.OnZombieAttack += OnZombieAttacks;
        combat.OnZombieDamageTaken += OnZombieGetHurts;
        combat.OnZombieDead += OnZombieDie;
    }

    private void OnDisable()
    {
        combat.OnZombieAttack -= OnZombieAttacks;
        combat.OnZombieDamageTaken -= OnZombieGetHurts;
        combat.OnZombieDead -= OnZombieDie;
    }

    public void IdleManager()
    {
        animator.SetTrigger("IdleTrigger");
    }

    public void WalkManager()
    {
        animator.SetTrigger("WalkTrigger");
    }

    public void RunManager()
    {
        animator.SetTrigger("RunTrigger");
    }

    public void AttackManager()
    {
        animator.SetInteger("AttackIndex", Random.Range(0, 1));
        animator.SetTrigger("AttackTrigger");
    }

    private void Initialize()
    {
        animator.SetInteger("IdleIndex", Random.Range(0, 3));
        animator.SetInteger("WalkIndex", Random.Range(0, 1));
        animator.SetInteger("RunIndex", Random.Range(0, 1));
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (!movement.IsMoving(0.2f)) IdleManager();
        else
        {
            if (movement.GetIsPlayerDetected())
                RunManager();

            if (!movement.GetIsPlayerDetected())
                WalkManager();            
        }

        animator.SetBool("IsMoving", movement.IsMoving(0.2f));
    }

    #region Event Triggers

    /// <summary>
    /// Triggers when zombie do a attack
    /// </summary>
    private void OnZombieAttacks()
    {
        AttackManager();

    }

    /// <summary>
    /// Triggers when zombie get hurts
    /// </summary>
    private void OnZombieGetHurts(FPSItem item)
    {

    }

    /// <summary>
    /// Triggers when zombie die
    /// </summary>
    private void OnZombieDie()
    {

    }

    #endregion
}
