using UnityEngine;

public class DragonFireBreath : MonoBehaviour
{
    [Header("Fire Settings")]
    [SerializeField] private GameObject firePrefab;
    [SerializeField] private Transform firePoint;

    private GameObject currentFireInstance;

    public void PlayFireBreath()
    {
        if (firePrefab == null || firePoint == null) return;

        if (currentFireInstance != null)
            Destroy(currentFireInstance);

        currentFireInstance = Instantiate(firePrefab, firePoint.position, firePoint.rotation);
        currentFireInstance.transform.SetParent(firePoint);
    }

    public void StopFireBreath()
    {
        if (currentFireInstance != null)
        {
            Destroy(currentFireInstance);
            currentFireInstance = null;
        }
    }
}