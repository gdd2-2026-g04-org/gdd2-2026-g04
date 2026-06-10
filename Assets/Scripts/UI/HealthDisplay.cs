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
    [SerializeField] private Image damageBar;
    
    private float delayTimer;
    private float lerpSpeed = 0.2f; 

    private void Start()
    {
      if (target == null) return;
      target.OnHealthChanged += UpdateDisplay;
      UpdateDisplay(target.CurrentHP, target.MaxHP);
      
      if (damageBar != null) damageBar.fillAmount = (float)target.CurrentHP / target.MaxHP;
    }

    private void OnDestroy()
    {
      if (target != null)
        target.OnHealthChanged -= UpdateDisplay;
    }

    private void Update()
    {
        if (damageBar != null && damageBar.fillAmount > fillBar.fillAmount)
        {
            if (delayTimer > 0)
            {
                delayTimer -= Time.deltaTime;
            }
            else
            {
                damageBar.fillAmount -= lerpSpeed * Time.deltaTime;
            }
        }
    }

private void UpdateDisplay(int current, int max)
{
    Debug.Log($"{gameObject.name} (Target: {target.name}) updated to {current}/{max}");

    if (label != null)
        label.text = $"{current} / {max}";

      // 2. Update the Bars
      if (fillBar != null)
      {
        float ratio = (max > 0) ? (float)current / (float)max : 0f;
        
        // Debugging line (Check your console to see if the ratio makes sense)
        Debug.Log($"Displaying {gameObject.name}: {current}/{max} = {ratio}");

        // If the health dropped, trigger the delay for the orange bar
        if (ratio < fillBar.fillAmount)
        {
            delayTimer = 0.5f; 
        }
        
        fillBar.fillAmount = ratio;
      }
    }
  }
}