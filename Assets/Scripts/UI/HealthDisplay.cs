using System;
using System.Collections;
using GameAssets.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssets.UI
{
  public class HealthDisplay : MonoBehaviour
  {
    private enum TargetType
    {
        Player,
        Boss
    }

    [Header("Target")]
    [SerializeField] private TargetType targetType;

    [Header("Counter")]
    [SerializeField] private TMP_Text label;

    [Header("Health Bars")]
    [SerializeField] private Image fillBar;
    [SerializeField] private Image damageBar;

    [Header("Damage Bar Animation")]
    [SerializeField, Min(0f)] private float damageBarDelay = 0.5f;
    [SerializeField, Min(0f)] private float damageBarSpeed = 0.2f;

    private HealthComponent target;
    private Coroutine resolveTargetCoroutine;
    private float delayTimer;

    private void OnEnable()
    {
        resolveTargetCoroutine = StartCoroutine(ResolveTargetWhenAvailable());
    }

    private void OnDisable()
    {
        if (resolveTargetCoroutine != null)
        {
            StopCoroutine(resolveTargetCoroutine);
            resolveTargetCoroutine = null;
        }
        UnsubscribeFromTarget();
    }

    private void Update()
    {
        if (!fillBar || !damageBar) return;

        if (damageBar.fillAmount <= fillBar.fillAmount) return;

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        damageBar.fillAmount =
            Mathf.MoveTowards(damageBar.fillAmount, fillBar.fillAmount, damageBarSpeed * Time.deltaTime);
    }

    private IEnumerator ResolveTargetWhenAvailable()
    {
        while (!target)
        {
            var resolvedTarget = ResolveTarget();

            if (resolvedTarget && resolvedTarget.IsSpawned)
            {
                SetTarget(resolvedTarget);
                resolveTargetCoroutine = null;
                yield break;
            }

            yield return null;
        }
        resolveTargetCoroutine = null;
    }

    private HealthComponent ResolveTarget()
    {
        switch (targetType)
        {
            case TargetType.Player:
                return NetworkManager.Instance ? NetworkManager.Instance.LocalPlayerHealth : null;

            case TargetType.Boss:
                var healthManager = FindFirstObjectByType<HealthSystemManager>();
                return healthManager != null ? healthManager.Boss : null;

            default:
                return null;
        }
    }

    private void SetTarget(HealthComponent newTarget)
    {
        if (target == newTarget) return;

        UnsubscribeFromTarget();
        target = newTarget;

        if (target != null)
        {
            target.OnHealthChanged += UpdateDisplay;
            UpdateDisplay(target.CurrentHP, target.MaxHP);
        }
        else
        {
            ClearDisplay();
        }
    }

    private void UnsubscribeFromTarget()
    {
        if (target) target.OnHealthChanged -= UpdateDisplay;
        target = null;
    }

    private void UpdateDisplay(int currentHP, int maxHP)
    {
        var ratio = maxHP > 0 ? Mathf.Clamp01((float)currentHP / maxHP) : 0f;
        if (label) label.text = $"{currentHP} / {maxHP}";

        if (fillBar)
        {
            if (ratio < fillBar.fillAmount) delayTimer = damageBarDelay;
            fillBar.fillAmount = ratio;
        }

        if (damageBar)
        {
            if (ratio >= damageBar.fillAmount)
            {
                damageBar.fillAmount = ratio;
            }
        }
    }

    private void ClearDisplay()
    {
        if (label) label.text = "0 / 0";
        if (fillBar) fillBar.fillAmount = 0f;
        if (damageBar) damageBar.fillAmount = 0f;
        delayTimer = 0f;
    }
  }
}