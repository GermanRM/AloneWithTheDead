using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{

    #region Player Detection
    [Header("Player Detection Properties")]
    private bool isPlayerDetected;
    
    [Tooltip("Time needed to detect the player when is visible")]
    [SerializeField] private float detectionTime;
    private float detectionTimeCounter;

    [Tooltip("Time needed to forget the player when is detected")]
    [SerializeField] private float forgetTime;
    private float forgetTimeCounter;
    #endregion

    #region Movement
    [Header("Movement")]
    [SerializeField] private Transform playerTarget;
    #endregion

    #region Patrolling
    [Header("Patrolling Properties")]
    [SerializeField] private Transform centerPoint;

    [Tooltip("How far from the centerPoint the zombie goes")]
    [SerializeField] private float patrollingDistance;
    bool goToNextPoint = true;
    #endregion

    #region Combat

    [Header("Combat Properties")]
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackReach;
    private bool canAttack = true;

    #endregion

    [Header("References")]
    [SerializeField] ZombieReferences references;

    private void Awake()
    {
        if (playerTarget == null) playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (playerTarget == null) playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerDetection();

        Movement();
        ZombieAttack();
    }

    #region Player Detection

    private void PlayerDetection()
    {
        DetectionTime();
        ForgetPlayer();
    }

    private void DetectionTime()
    {
        if (!isPlayerDetected)
        {
            if (references.fov.CanSeePlayer())
                detectionTimeCounter -= Time.deltaTime;
            else
                detectionTimeCounter += Time.deltaTime;
        }

        detectionTimeCounter = Mathf.Clamp(detectionTimeCounter, 0, detectionTime);

        if (detectionTimeCounter <= 0)
            isPlayerDetected = true;
    }

    private void ForgetPlayer()
    {
        if (isPlayerDetected)
        {
            if (!references.fov.CanSeePlayer())
            {
                forgetTimeCounter -= Time.deltaTime;
            }
            else
            {
                forgetTimeCounter = forgetTime;
            }
        }

        forgetTimeCounter = Mathf.Clamp(forgetTimeCounter, 0, forgetTime);

        if (forgetTimeCounter <= 0)
            isPlayerDetected = false;
    }

    #endregion

    #region Movement

    private void Movement()
    {
        if (isPlayerDetected)
        {
            references.agent.SetDestination(playerTarget.transform.position);
        }
        else
        {

            if (references.agent.remainingDistance <= references.agent.stoppingDistance && !references.agent.pathPending)
            {

                //if a center point is defined
                if (centerPoint != null)
                {
                    if (goToNextPoint) //done with path
                    {
                        Vector3 point;
                        if (RandomPoint(centerPoint.position, patrollingDistance, out point)) //pass in our centre point and radius of area
                        {
                            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                            references.agent.SetDestination(point);
                        }
                    }
                }
                else // if a center point is not defined
                {
                    if (goToNextPoint) //done with path
                    {
                        Vector3 point;
                        if (RandomPoint(transform.position, 5, out point)) //pass in our centre point and radius of area
                        {
                            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                            references.agent.SetDestination(point);
                        }
                    }
                }
 
            }
        }
    }

    #region Checkers / Utility
    /// <summary>
    /// Give us a random point in the nav mesh area to do random patrolling
    /// </summary>
    /// <param name="center"></param>
    /// <param name="range"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Delay to activate go to next point variable
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private IEnumerator GoNextPointDelay(float seconds)
    {
        goToNextPoint = false;

        yield return new WaitForSeconds(seconds);

        goToNextPoint = true;
    }

    #endregion

    #endregion

    #region Combat

    private void ZombieAttack()
    {
        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (distance <= attackReach && isPlayerDetected && canAttack)
        {
            Debug.Log("Attack: " + attackDamage);
            StartCoroutine(AttackCooldownCoroutine(attackCooldown));
        }
    }

    private IEnumerator AttackCooldownCoroutine(float seconds)
    {
        canAttack = false;
        yield return new WaitForSeconds(seconds);
        canAttack = true;
    }

    #endregion
}
