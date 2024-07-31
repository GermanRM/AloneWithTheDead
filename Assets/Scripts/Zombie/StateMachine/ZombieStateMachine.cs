using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieStateMachine : MonoBehaviour
{
    [Header("State Machine Properties")]
    [Tooltip("Zombie State")]
    [SerializeField] private ZombieState zombieState = ZombieState.Idle;

    [Header("State Machine Variables")]
    [SerializeField] private bool IsMoving;
    [SerializeField] private bool IsFollowing;
    [SerializeField] private bool IsPatrolling;
    [SerializeField] private bool IsBoring;

    [Space]
    [SerializeField] private List<GameObject> zombieModels = new List<GameObject>();

    private Animator stateMachine;
    private ZombieMovement zombieMovement;

    private void Awake()
    {
        stateMachine = GetComponent<Animator>();
        zombieMovement = GetComponent<ZombieMovement>();

        InitializeModels();
    }

    private void InitializeModels()
    {
        foreach (var model in zombieModels)
        {
            model.SetActive(false);
        }

        zombieModels[Random.Range(0, zombieModels.Count)].SetActive(true);
    }

    #region Getter / Setter

    public void SetZombieState(ZombieState state) { this.zombieState = state; }
    public ZombieState GetZombieState() { return this.zombieState; }

    public bool GetIsMoving() { return this.IsMoving; }
    public bool GetIsFollowing() {  return this.IsFollowing; }
    public bool GetIsPatrolling() { return this.IsPatrolling; }
    public bool GetIsBoring() { return this.IsBoring; }

    #endregion

    // Update is called once per frame
    void Update()
    {
        StateMachineVariables();
    }

    private void StateMachineVariables()
    {
        IsMoving = zombieMovement.IsMoving(0.2f);
        IsFollowing = zombieMovement.GetIsFollowing();
        IsPatrolling = IsMoving && !IsFollowing;
        IsBoring = !IsMoving && !IsFollowing;

        stateMachine.SetBool("isMoving", IsMoving);
        stateMachine.SetBool("isFollowing", IsFollowing);
        stateMachine.SetBool("isPatrolling", IsPatrolling);
        stateMachine.SetBool("isBoring", IsBoring);
    }
}
