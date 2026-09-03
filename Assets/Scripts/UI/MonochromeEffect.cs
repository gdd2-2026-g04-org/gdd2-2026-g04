using System;
using System.Collections;
using GameAssets.Health;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class MonochromeEffect : MonoBehaviour
{
    private Volume volume;
    [SerializeField] private float fadeDuration = 0.6f;
    
    private ColorAdjustments colorAdjustments;
    private PlayerHealth playerHealth;
    private Coroutine fadeRoutine;
    private bool isDead;

    private void Awake()
    {
        volume = GetComponent<Volume>();

        if (!volume)
        {
            Debug.LogError("Missing Volume!");
            return;
        }

        if (!volume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("Missing color adjustments!");
            return;
        }
        
        colorAdjustments.saturation.overrideState = true;
        colorAdjustments.saturation.value = 15f;
        colorAdjustments.contrast.overrideState = true;
        colorAdjustments.contrast.value = 10f;
    }

    private void OnEnable()
    {
        StartCoroutine(BindToLocalPlayerHealth());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (playerHealth)
        {
            playerHealth.OnDeath -= OnDeath;
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    private IEnumerator BindToLocalPlayerHealth()
    {
        while (!NetworkManager.Instance || !NetworkManager.Instance.LocalPlayerHealth)
        {
            yield return null;
        }

        playerHealth = NetworkManager.Instance.LocalPlayerHealth;

        playerHealth.OnDeath += OnDeath;
        playerHealth.OnHealthChanged += OnHealthChanged;

        if (!playerHealth.IsAlive)
        {
            FadeTo(-100f, 30f);
        }
        else
        {
            FadeTo(15f, 10f);
        }
    }

    private void OnDeath()
    {
        FadeTo(-100f, 30f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LobbyScene") FadeTo(15f, 10f);
    }

    private void OnHealthChanged(int curHP, int maxHP)
    {
        if (curHP > 0)
        {
            FadeTo(15f, 10f);
        }
    }

    private void FadeTo(float targetSat, float targetContrast)
    {
        if (!colorAdjustments) return;
        
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        
        fadeRoutine = StartCoroutine(FadeRoutine(targetSat, targetContrast));
    }

    private IEnumerator FadeRoutine(float targetSat, float targetContrast)
    {
        var startSat = colorAdjustments.saturation.value;
        var startContrast = colorAdjustments.contrast.value;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            var timer = t / fadeDuration;
            
            colorAdjustments.saturation.value = Mathf.Lerp(startSat, targetSat, timer);
            colorAdjustments.contrast.value = Mathf.Lerp(startContrast, targetContrast, timer);
            yield return null;
        }
        
        colorAdjustments.saturation.value = targetSat;
        colorAdjustments.contrast.value = targetContrast;
        fadeRoutine = null;
    }
}
