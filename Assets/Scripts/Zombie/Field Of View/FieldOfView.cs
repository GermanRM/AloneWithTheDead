using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Field Of View Properties")]

    [Tooltip("Total Area of view")]
    [SerializeField] private float viewRadius;

    [Tooltip("Angle of view")]
    [SerializeField] private float viewAngle;

    [Tooltip("Mask of the target")]
    [SerializeField] private LayerMask targetMask;

    [Tooltip("Mask of the obstacles")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("List of targets targeted")]
    [SerializeField] private List<Transform> visibleTargets = new List<Transform>();

    [Header("Debug Properties")]
    [Tooltip("Enable to see the fov in gizmos")]
    [SerializeField] private bool seeFOV;

    #region Getter / Setter

    public bool CanSeePlayer() { return visibleTargets.Count > 0; }

    public float GetViewRadius() {  return viewRadius; }
    public float GetViewAngle() { return viewAngle; }

    public LayerMask GetTargetMask() { return targetMask; }
    public LayerMask GetObstacleMask() { return obstacleMask; }

    public List<Transform> GetVisibleTargets() { return visibleTargets; }

    #endregion

    private void Start()
    {
        StartCoroutine(FOVDelay(0.2f));
    }

    private void Update()
    {
        //FindVisibleTargets();
    }

    /// <summary>
    /// We use this IEnumerator to delay the fov (save perfomance)
    /// </summary>
    /// <returns></returns>
    private IEnumerator FOVDelay(float seconds)
    {
        while (true)
        {
            FindVisibleTargets();
            yield return new WaitForSeconds(seconds);
        }
    }

    /// <summary>
    /// Find Targets in AI Vision
    /// </summary>
    private void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    #region Checkers / Utility

    /// <summary>
    /// Transform angle to direction
    /// </summary>
    /// <param name="angleInDegrees"></param>
    /// <param name="angleIsGlobal"></param>
    /// <returns></returns>
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (seeFOV)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, viewRadius);

            Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
            Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

            Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);

            Gizmos.color = Color.red;
            foreach (Transform visibleTarget in visibleTargets)
            {
                Gizmos.DrawLine(transform.position, visibleTarget.position);
            }
        }
    }
}
