using System.Collections;
using GameAssets.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssets.UI
{
    public class ManaDisplay : MonoBehaviour
    {
        [Header("Counter")]
        [SerializeField] private TMP_Text label;

        [Header("Mana Bars")]
        [SerializeField] private Image fillBar;
        [SerializeField] private Image damageBar;

        [Header("Damage Bar Animation")]
        [SerializeField, Min(0f)] private float damageBarDelay = 0.5f;
        [SerializeField, Min(0f)] private float damageBarSpeed = 0.2f;

        [Header("Visibility")]
        [SerializeField] private GameObject manaRoot;

        private MageMana target;
        private Coroutine resolveTargetCoroutine;
        private GameObject inferredBarRoot;
        private float delayTimer;

        private void OnEnable()
        {
            if (!manaRoot)
            {
                manaRoot = gameObject;
            }

            inferredBarRoot = ResolveBarRoot();
            resolveTargetCoroutine = StartCoroutine(ResolveTargetWhenAvailable());
            RefreshVisibility(force: true);
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
            RefreshVisibility(force: false);

            if (!IsVisible())
            {
                ClearDisplay();
                return;
            }

            if (!target)
            {
                var resolvedTarget = ResolveTarget();
                if (resolvedTarget)
                {
                    SetTarget(resolvedTarget);
                }
            }

            if (target)
            {
                UpdateDisplay(target.CurrentMana, target.MaxMana);
            }

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

        private void RefreshVisibility(bool force)
        {
            var shouldShow = IsLocalMage();

            SetActiveIfNeeded(manaRoot, shouldShow);
            SetActiveIfNeeded(inferredBarRoot, shouldShow);

            if (!shouldShow) ClearDisplay();
        }

        private bool IsVisible()
        {
            var rootVisible = !manaRoot || manaRoot.activeSelf;
            var barsVisible = !inferredBarRoot || inferredBarRoot.activeSelf;
            return rootVisible && barsVisible;
        }

        private IEnumerator ResolveTargetWhenAvailable()
        {
            while (!target)
            {
                var resolvedTarget = ResolveTarget();

                if (resolvedTarget)
                {
                    SetTarget(resolvedTarget);
                    resolveTargetCoroutine = null;
                    yield break;
                }

                yield return null;
            }

            resolveTargetCoroutine = null;
        }

        private MageMana ResolveTarget()
        {
            return FindFirstObjectByType<MageMana>();
        }

        private void SetTarget(MageMana newTarget)
        {
            if (target == newTarget) return;

            UnsubscribeFromTarget();
            target = newTarget;

            if (target != null)
            {
                UpdateDisplay(target.CurrentMana, target.MaxMana);
            }
            else
            {
                ClearDisplay();
            }
        }

        private void UnsubscribeFromTarget()
        {
            target = null;
        }

        private void SetActiveIfNeeded(GameObject uiTarget, bool state)
        {
            if (!uiTarget) return;
            if (uiTarget.activeSelf == state) return;
            uiTarget.SetActive(state);
        }

        private GameObject ResolveBarRoot()
        {
            if (!fillBar || !damageBar) return null;

            var fillParent = fillBar.transform.parent;
            var damageParent = damageBar.transform.parent;
            if (!fillParent || fillParent != damageParent) return null;

            var root = fillParent.gameObject;
            if (root == manaRoot) return null;
            return root;
        }

        private bool IsLocalMage()
        {
            if (NetworkManager.Instance)
            {
                var localHealth = NetworkManager.Instance.LocalPlayerHealth;
                if (localHealth && localHealth.CurrentClass != PlayerClass.None)
                {
                    return localHealth.CurrentClass == PlayerClass.Mage;
                }
            }

            if (LocalClassSelector.Instance && LocalClassSelector.Instance.SelectedClass != PlayerClass.None)
            {
                return LocalClassSelector.Instance.SelectedClass == PlayerClass.Mage;
            }

            return false;
        }

        private void UpdateDisplay(int currentMana, int maxMana)
        {
            var ratio = maxMana > 0 ? Mathf.Clamp01((float)currentMana / maxMana) : 0f;

            if (label) label.text = $"{currentMana} / {maxMana}";

            if (fillBar)
            {
                if (ratio < fillBar.fillAmount) delayTimer = damageBarDelay;
                fillBar.fillAmount = ratio;
            }

            if (damageBar && ratio >= damageBar.fillAmount)
            {
                damageBar.fillAmount = ratio;
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
