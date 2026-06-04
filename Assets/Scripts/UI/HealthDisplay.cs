using GameAssets.Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssets.UI
{
  public class HealthDisplay : MonoBehaviour
  {
    [SerializeField] private HealthComponent target;

    [Header("Counter")]
    [SerializeField] private TMP_Text label;

    [Header("Bar")]
    [SerializeField] private Image fillBar;

    private void Start()
    {
      if (target == null) return;
      target.OnHealthChanged += UpdateDisplay;
      UpdateDisplay(target.CurrentHP, target.MaxHP);
    }

    private void OnDestroy()
    {
      if (target != null)
        target.OnHealthChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int current, int max)
    {
      if (label != null)
        label.text = $"{current} / {max}";

      if (fillBar != null)
        fillBar.fillAmount = max > 0 ? (float)current / max : 0f;
    }
  }
}
