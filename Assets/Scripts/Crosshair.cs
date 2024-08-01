using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Range(0, 100)]
    public float value;
    public float speed;

    public float delay;
    public float margin;
    public float multiplier;
    public GameObject player;
    public RectTransform top, bottom, left, right, center;

    [Header("Script References")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerMovement playerMovement;
    void Update()
    {
        value = player.GetComponent<PlayerMovement>().GetMoveVelocityMagnitude() * multiplier;

        float TopValue, BottomValue, LeftValue, RightValue;

        TopValue = Mathf.Lerp(top.position.y, center.position.y + margin + value, speed * Time.deltaTime);
        BottomValue = Mathf.Lerp(bottom.position.y, center.position.y - margin - value, speed * Time.deltaTime);
        LeftValue = Mathf.Lerp(left.position.x, center.position.x - margin - value, speed * Time.deltaTime);
        RightValue = Mathf.Lerp(right.position.x, center.position.x + margin + value, speed * Time.deltaTime);

        top.position = new Vector2(top.position.x, TopValue);
        bottom.position = new Vector2(bottom.position.x, BottomValue);
        left.position = new Vector2(LeftValue, center.position.y);
        right.position = new Vector2(RightValue, center.position.y);
    }

    private void OnEnable()
    {
        stats.OnPlayerAttackAnimStart += OnPlayerAttacks;
        playerMovement.OnPlayerLand += OnPlayerLands;
    }

    private void OnDisable()
    {
        stats.OnPlayerAttackAnimStart -= OnPlayerAttacks;
        playerMovement.OnPlayerLand += OnPlayerLands;
    }

    private void OnPlayerAttacks(FPSItem item)
    {
        StartCoroutine(MultiplierDelay(delay));
    }
    private void OnPlayerLands()
    {
        StartCoroutine(DividerDelay(delay));
    }
    private IEnumerator MultiplierDelay(float seconds)
    {
        multiplier *= 2.5f;
        yield return new WaitForSeconds(seconds);
        multiplier /= 2.5f;
    }
    private IEnumerator DividerDelay(float seconds)
    {
        multiplier *= -1.8f;
        yield return new WaitForSeconds(seconds);
        multiplier /= -1.8f;
    }
}
