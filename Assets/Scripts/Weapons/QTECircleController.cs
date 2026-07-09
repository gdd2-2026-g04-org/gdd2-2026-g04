using System;
using GameAssets.Health;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QTECircleController : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject circlePrefab;

    [Header("Bow")]
    public BowController bow;

    [Header("Placement")]
    [Range(0f, 1f)]
    public float spawnPercent = 0.8f;
    public float heightOffset = 3.0f;

    [Header("Reduction")]
    public float initialScale = 2f;
    public float duration = 3f;

    public bool IsAimInside {  get; private set; }
    public bool TimeOut { get; private set; }

    private Transform playerCamera;
    private BossHealth boss;

    private GameObject currentCircle;

    private float timer;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        ResolveReferences();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        HideCircle();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        boss = null;
        
        HideCircle();
        ResolveReferences();
    }
    
    public void ShowCircle()
    {
        if (currentCircle != null)
            return;
        
        ResolveReferences();

        Vector3 position = Vector3.Lerp(playerCamera.position, boss.transform.position, spawnPercent);
        
        position += Vector3.up * heightOffset;

        currentCircle = Instantiate(circlePrefab, position, Quaternion.identity);

        currentCircle.transform.LookAt(playerCamera.position, Vector3.up);

        currentCircle.transform.localScale = Vector3.one * initialScale;

        TimeOut = false;
        IsAimInside = false;
        timer = duration;

    }

    void Update()
    {
        if (currentCircle == null)
            return;

        timer -= Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);

        currentCircle.transform.localScale =
            Vector3.one * (initialScale * t);
        
        CheckAim();

        if (timer <= 0f)
        {
            TimeOut = true;
            HideCircle();
        }
    }


    public void HideCircle()
    {
        if (currentCircle)
        {
            Destroy(currentCircle);
            currentCircle = null;
        }

        IsAimInside = false;
    }

    private void ResolveReferences()
    {
        if (XRReferences.Instance) playerCamera = XRReferences.Instance.head;
        
        if (!boss) boss = FindFirstObjectByType<BossHealth>();
    }

    private void CheckAim()
    {
        if (!currentCircle || !bow || bow.AimDirection.sqrMagnitude <= 0.0001f)
        {
            IsAimInside = false;
            return;
        }
        
        Plane plane = new Plane(
            currentCircle.transform.forward,
            currentCircle.transform.position);

        Ray ray = new Ray(
            bow.AimOrigin,
            bow.AimDirection);

        if (!plane.Raycast(ray, out float distance))
        {
            IsAimInside = false;
            return;
        }

        Vector3 hitPoint = ray.GetPoint(distance);

        float currentRadius =
            currentCircle.transform.localScale.x * 0.5f;

        float offset =
            Vector3.Distance(hitPoint,
                             currentCircle.transform.position);

        IsAimInside = offset <= currentRadius;
    }

}
