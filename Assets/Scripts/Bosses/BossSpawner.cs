using Fusion;
using UnityEngine;
using GameAssets.Health;

public class BossSpawner : NetworkBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private NetworkObject[] bossPrefabs;
    [SerializeField] private Transform spawnPoint;

    private NetworkObject currentBoss;

    public override void Spawned()
    {
        if (Object.HasStateAuthority && bossPrefabs != null && bossPrefabs.Length > 0)
        {
            SpawnBoss(Random.Range(0, bossPrefabs.Length));
        }
    }

    public void SpawnBoss(int index)
    {
        if (!Object.HasStateAuthority || bossPrefabs == null || index < 0 || index >= bossPrefabs.Length)
            return;

        if (currentBoss != null)
            Runner.Despawn(currentBoss);

        // Use spawnPoint if assigned, otherwise use a default position
        Vector3 position = spawnPoint != null 
            ? spawnPoint.position + new Vector3(0, 1.5f, 0) 
            : new Vector3(0, 2f, 8f);

        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        currentBoss = Runner.Spawn(bossPrefabs[index], position, rotation);

        // Register with HealthSystemManager
        var bossHealth = currentBoss?.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            FindFirstObjectByType<HealthSystemManager>()?.RegisterBoss(bossHealth);
        }

        Debug.Log($"(BossSpawner) Spawned {bossPrefabs[index].name}");
    }
}