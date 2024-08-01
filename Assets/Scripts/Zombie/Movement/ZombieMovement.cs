using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieMovement : MonoBehaviour
{
    [Header("Movement Properties")]

    [Tooltip("Zombie Movement Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [Header("Target Properties")]
    [Tooltip("Movement Target")]
    [SerializeField] private Vector3 targetPos;

    [Header("Detection Properties")]
    [SerializeField] private Transform targetTransform;

    [Tooltip("Determine if player is Visible to the zombie")]
    [SerializeField] private bool isPlayerVisible;

    [Tooltip("Determine if player has been detected")]
    [SerializeField] private bool isPlayerDetected;

    [Tooltip("Turn speed when player is detected")]
    [SerializeField] private float turnSpeed;

    [Tooltip("Amount of time until the enemy forget the player location")]
    [SerializeField] private float forgetTimeTolerance;
    private float forgetCounter;

    [Tooltip("Amount of time the enemy must see the player to determine that he has detected him (When is far)")]
    [SerializeField] private float visionWatchTimeTolerance;

    [Tooltip("Amount of time the enemy must see the player to determine that he has detected him (When is in blindSpot)")]
    [SerializeField] private float closeWatchTimeTolerance;

    private float watchTimeTolerance;
    private float watchTimeCounter = 0; //Counter of watchTime

    [Header("Patroll Properties")]
    [SerializeField] private Transform centrePoint;

    [Header("Blind Spot Detection Properties")]
    [SerializeField] private Transform blindSpot;
    [SerializeField] private Vector3 blindSpotSize;
    [SerializeField] private float blindSpotDistance;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask defaultLayer;

    /// <summary>
    /// Delay to go to the next point
    /// </summary>
    [SerializeField] private float minRandomMovementDelay;
    [SerializeField] private float maxRandomMovementDelay;

    /// <summary>
    /// Random range of max movement range
    /// </summary>
    [SerializeField] private float minRandomMovementRange;
    [SerializeField] private float maxRandomMovementRange;

    bool goToNextPoint = true;
    bool isFollowing = false;

    //Script References
    private FieldOfView FOV;
    private NavMeshAgent agent;
    private ZombieCombat combat;

    private void Awake()
    {
        FOV = GetComponent<FieldOfView>();
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<ZombieCombat>();
    }

    /// <summary>
    /// Initialize zombie with this variables
    /// </summary>
    private void InitializeZombie()
    {
        agent.speed = walkSpeed;
    }

    #region Getter / Setter

    public NavMeshAgent GetAgent() { return agent; }

    public Vector3 GetTargetPos() { return targetPos; }

    public Transform GetTargetTransform() { return targetTransform; }
    public void SetTargetTransform(Transform transform) {  targetTransform = transform; }

    public bool GetIsPlayerDetected() { return isPlayerDetected; }
    public void SetIsPlayerDetected(bool detected) { this.isPlayerDetected = detected; }

    public bool IsMoving(float minSpeed) { return agent.velocity.magnitude > minSpeed; }

    public bool GetIsFollowing() { return isFollowing; }

    public bool GetIsPlayerVisible() { return isPlayerVisible; }

    #endregion

    // Update is called once per frame
    void Update()
    {
        WatchTimeCounterManager();
        ForgetPlayerLocation();

        Movement();
    }

    #region Zombie Movement

    /// <summary>
    /// Manage all zombie movement
    /// </summary>
    private void Movement()
    {
        if (isPlayerDetected)
        {
            agent.speed = runSpeed;
            isFollowing = true;
            targetPos = GetDetectedPlayerPos();
            agent.SetDestination(targetPos);

            if (CanAttack(true))
            {
                RotateToPlayer();

                //Attack
            }

        }
        else
        {
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {               
                agent.speed = walkSpeed;
                isFollowing = false;

                if (goToNextPoint) //done with path
                {
                    Vector3 point;
                    if (RandomPoint(centrePoint.position, Random.Range(minRandomMovementRange, maxRandomMovementRange), out point)) //pass in our centre point and radius of area
                    {
                        Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                        agent.SetDestination(point);
                        StartCoroutine(GoNextPointDelay(Random.Range(minRandomMovementDelay, maxRandomMovementDelay)));
                    }
                }
            }
        }
    }

    #endregion

    #region Player Detector

    /// <summary>
    /// Manages Watch Time Counter variables
    /// </summary>
    private void WatchTimeCounterManager()
    {
        if (targetTransform == null)
        {
            if (FOV.GetVisibleTargets().Count > 0)
            {
                targetTransform = FOV.GetVisibleTargets()[0];
            }

            if (Physics.BoxCast(blindSpot.position, blindSpotSize, transform.forward, out RaycastHit hit, transform.rotation, blindSpotDistance, defaultLayer | enemyLayer))
            {
                if (hit.collider.CompareTag("Player")) targetTransform = hit.transform.gameObject.transform;
            }
        }

        if (DetectBlindSpot())
        {
            watchTimeTolerance = closeWatchTimeTolerance;
        }
        else
        {
            watchTimeTolerance = visionWatchTimeTolerance;
        }

        isPlayerVisible = FOV.GetVisibleTargets().Count > 0 || DetectBlindSpot();

        if (isPlayerVisible) //if the player is visible
        {
            watchTimeCounter = Mathf.Clamp(watchTimeCounter -= Time.deltaTime, 0, watchTimeTolerance);
        }
        else
        {
            watchTimeCounter = Mathf.Clamp(watchTimeCounter += Time.deltaTime, 0, watchTimeTolerance);
        }

        if (watchTimeCounter <= 0 && !isPlayerDetected)
        {
            isPlayerDetected = true; //if the watchTimeCounter reach zero or less, then player is detected, otherwise is not detected
        }      
    }

    private void ForgetPlayerLocation()
    {
        if (isPlayerDetected)
        {
            if (!isPlayerVisible) forgetCounter = Mathf.Clamp(forgetCounter -= Time.deltaTime, 0, forgetTimeTolerance);
            else forgetCounter = forgetTimeTolerance;

        }
        else
        {
            forgetCounter = Mathf.Clamp(forgetCounter += Time.deltaTime, 0, forgetTimeTolerance);
        }

        if (forgetCounter <= 0 && isPlayerDetected && !isPlayerVisible)
        {
            isPlayerDetected = false;
        }
    }

    #endregion

    #region Checkers / Utilities

    private void RotateToPlayer()
    {
        if (targetTransform != null && combat.isAlive)
        {
            //Rotate the zombie to the player
            // Calcular la dirección hacia el jugador
            Vector3 direction = (targetTransform.position - transform.position).normalized;

            // Rotar el agente para que mire al jugador
            if (direction != Vector3.zero) // Evitar errores de división por cero
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
            }
        }
    }

    public bool CanAttack(bool canAttack)
    {
        return isPlayerVisible && DetectBlindSpot() && !agent.pathPending && canAttack;
    }

    public bool DetectBlindSpot()
    {
        return Physics.BoxCast(blindSpot.position, blindSpotSize, transform.forward, transform.rotation, blindSpotDistance, defaultLayer | enemyLayer);
    }

    /// <summary>
    /// Get the player pos
    /// </summary>
    /// <returns></returns>
    public Vector3 GetDetectedPlayerPos() {  return targetTransform.position; }

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

    #region Debug

    private void OnDrawGizmos()
    {
        ExtDebug.DrawBoxCastBox(blindSpot.position, blindSpotSize, transform.rotation, transform.forward, blindSpotDistance, DetectBlindSpot() ? Color.green : Color.red);
    }

    #endregion
}
