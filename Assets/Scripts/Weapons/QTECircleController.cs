using UnityEngine;

public class QTECircleController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform boss;

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

    private GameObject currentCircle;

    private float timer;

    public void ShowCircle()
    {
        if (currentCircle != null)
            return;

        Vector3 position = Vector3.Lerp(playerCamera.position, boss.position, spawnPercent);
        position += Vector3.up * heightOffset;

        currentCircle = Instantiate(circlePrefab, position, Quaternion.identity);

        currentCircle.transform.LookAt(playerCamera);

        currentCircle.transform.localScale = Vector3.one * initialScale;

        TimeOut = false;

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

        if (timer <= 0f)
        {
            TimeOut = true;
            HideCircle();
        }

        CheckAim();

    }


    public void HideCircle()
    {
        if (currentCircle == null)
            return;

        Destroy(currentCircle);
        currentCircle = null;
    }

    private void CheckAim()
    {
        if (currentCircle == null)
        {
            IsAimInside = false;
            return;
        }

        // Plano del círculo
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

        Debug.Log(IsAimInside);
    }

}
