using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimation : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] private Animator animator;
    [SerializeField] private ZombieStateMachine stateMachine;
    [SerializeField] private ZombieCombat combat;

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

    private void Initialize()
    {
        animator.SetInteger("IdleIndex", Random.Range(0, 3));
        animator.SetInteger("WalkIndex", Random.Range(0, 2));
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("IsBoring", stateMachine.GetIsBoring());
    }

    #region Event Triggers

    /// <summary>
    /// Triggers when zombie do a attack
    /// </summary>
    private void OnZombieAttacks()
    {

    }

    /// <summary>
    /// Triggers when zombie get hurts
    /// </summary>
    private void OnZombieGetHurts()
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
