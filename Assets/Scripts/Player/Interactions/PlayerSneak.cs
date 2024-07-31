using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSneak : MonoBehaviour
{
    [Header("Sneak Properties")]
    [SerializeField] private float actualRadius;
    [SerializeField] private float idleRadius;
    [SerializeField] private float walkRadius;
    [SerializeField] private float runRadius;
    [SerializeField] private float crouchRadius;
    [SerializeField] private float lerpSpeed;
    [SerializeField] private Transform noiseCenter;
    [SerializeField] private LayerMask zombieLayer;

    [Header("Script References")]
    [SerializeField] private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        actualRadius = walkRadius;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerMovement.IsMoving(0.2f))
        {
            actualRadius = Mathf.Lerp(actualRadius, idleRadius, lerpSpeed * Time.deltaTime);
        }
        else
        {
            if (playerMovement.IsCrouching())
                actualRadius = Mathf.Lerp(actualRadius, crouchRadius, lerpSpeed * Time.deltaTime);

            if (!playerMovement.GetIsRunning())
                actualRadius = Mathf.Lerp(actualRadius, walkRadius, lerpSpeed * Time.deltaTime);
            else
            {
                actualRadius = Mathf.Lerp(actualRadius, runRadius, lerpSpeed * Time.deltaTime);
            }
        }

        Collider[] colliders = Physics.OverlapSphere(noiseCenter.position, actualRadius, zombieLayer);

        foreach (Collider collider in colliders)
        {
            ZombieMovement zombieMovement = collider.gameObject.GetComponentInParent<ZombieMovement>();

            if (zombieMovement != null)
            {
                zombieMovement.SetTargetTransform(transform);
                zombieMovement.SetIsPlayerDetected(true);
            }
            else
            {
                Debug.Log("null");
            }
        }
    }

    public void MakeSneakNoise(float noiseRadius)
    {
        actualRadius = noiseRadius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(noiseCenter.position, actualRadius);
    }
}
