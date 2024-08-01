using System.Collections;
using System.Collections.Generic;
using Tensori.FPSHandsHorrorPack;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [Header("Crosshair Properties")]
    [Range(0, 100)][SerializeField] private float value;
    [SerializeField] private float speed;

    [Header("Crosshair Profiles")]
    [Range(0, 100)][SerializeField] private float idleValue;
    [SerializeField] private float crouchValue = 20f;
    [SerializeField] private float walkValue = 40f;
    [SerializeField] private float runValue = 60f;
    [SerializeField] private float meeleeMultiplier;
    [SerializeField] private float gunMultiplier;
    private bool modifyValue;

    [Header("Design Properties")]
    public float delay;
    public float margin;
    [SerializeField] private List<UnityEngine.UI.RawImage> crosshairImages;
    [SerializeField] private Color crosshairColor;
    [SerializeField] private RectTransform top, bottom, left, right, center;

    [Header("Script References")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        foreach (var item in crosshairImages)
        {
            item.color = crosshairColor;
        }
    }

    void Update()
    {
        EnableDisableOnAim();
        CrosshairMovement();
        OpenCrosshairInMovement();
    }

    private void OnEnable()
    {
        stats.OnPlayerAttackAnimStart += OnPlayerAttacks;
    }

    private void OnDisable()
    {
        stats.OnPlayerAttackAnimStart -= OnPlayerAttacks;
    }

    private void CrosshairMovement()
    {
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

    private void EnableDisableOnAim()
    {
        if (stats.isAiming)
        {
            top.gameObject.SetActive(false);
            bottom.gameObject.SetActive(false);
            left.gameObject.SetActive(false);
            right.gameObject.SetActive(false);
            center.gameObject.SetActive(false);
        }
        else
        {
            top.gameObject.SetActive(true);
            bottom.gameObject.SetActive(true);
            left.gameObject.SetActive(true);
            right.gameObject.SetActive(true);
            center.gameObject.SetActive(true);
        }
    }

    private void OpenCrosshairInMovement()
    {
        if (!modifyValue)
        {
            if (!playerMovement.IsMoving(0.2f))
            {
                if (!playerMovement.IsCrouching())
                    value = idleValue;
                else
                    value = crouchValue;
            }
            else
            {
                if (playerMovement.IsCrouching()) { }
                    value = crouchValue;

                if (!playerMovement.GetIsRunning())
                    value = walkValue;

                else
                {
                    value = runValue;
                }
            }

            value = Mathf.Clamp(value, 0f, 100f);
        }
    }

    private void OnPlayerAttacks(FPSItem item)
    {
        if (item.weaponType == FPSItem.WeaponType.Melee)
        {
            modifyValue = true;
            value *= meeleeMultiplier;
            modifyValue = false;
        }

        if (item.weaponType == FPSItem.WeaponType.Fire)
        {
            modifyValue = true;
            value *= gunMultiplier;
            modifyValue = false;
        }

    }
}
