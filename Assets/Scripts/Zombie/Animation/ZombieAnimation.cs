using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimation : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] private Animator animator;
    [SerializeField] private ZombieStateMachine stateMachine;

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
        
    }
}
