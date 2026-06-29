using UnityEngine;
using Fusion;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private static bool _hasStarted = false;

    private async void Start()
    {
        if (_hasStarted) return;
        _hasStarted = true;

        if (networkRunnerPrefab == null || !playerPrefab.IsValid)
        {
            Debug.LogError("Missing NetworkRunner Prefab or Player Prefab!");
            return;
        }

        var runner = Instantiate(networkRunnerPrefab);
        DontDestroyOnLoad(runner.gameObject);

        string roomName = "FinalStandRoom_" + Random.Range(1000, 9999);

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log($"✅ Successfully started as Host! Room: {roomName}");

            if (runner.IsServer)
            {
                if (!playerPrefab.IsValid)
                {
                    Debug.LogError("playerPrefab is INVALID!");
                    return;
                }

                // Clean world position in front of the player
                Vector3 spawnPos = new Vector3(0, 2f, 5f);

                Debug.Log($"[HOST SPAWN] Spawning at: {spawnPos}");

                var spawned = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, runner.LocalPlayer);

                if (spawned != null)
                {
                    Debug.Log("✅ Spawn succeeded!");

                    spawned.gameObject.name = "TEST_PLAYER_CUBE_VISIBLE";

                    // Do NOT parent it (causes transform corruption with NetworkTransform)
                    // Do NOT change scale here — change it on the prefab instead if needed

                    var mr = spawned.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.material.color = Color.magenta;
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"❌ Failed to start: {result.ErrorMessage}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && playerPrefab.IsValid)
        {
            Vector3 spawnPos = new Vector3(0, 3f, 0);
            Debug.Log($"[JOINED SPAWN] Spawning for {player} at: {spawnPos}");
            runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);   // ← Fixed: use 'player'
        }
    }
}