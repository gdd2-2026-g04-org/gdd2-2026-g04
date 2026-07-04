using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private NetworkRunner runner;

    private async void Start()
    {
        if (networkRunnerPrefab == null || !playerPrefab.IsValid)
        {
            Debug.LogError("Missing NetworkRunner Prefab or Player Prefab!");
            return;
        }

        runner = Instantiate(networkRunnerPrefab);
        runner.name = "NetworkRunner";
        DontDestroyOnLoad(runner.gameObject);

        runner.AddCallbacks(this);
        runner.ProvideInput = true;

        string roomName = "FinalStandRoom";

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log($"✅ Successfully started Fusion Shared Mode. Room: {roomName}");
            Debug.Log($"Local Player: {runner.LocalPlayer}");
        }
        else
        {
            Debug.LogError($"❌ Failed to start: {result.ErrorMessage}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[JOIN] Player joined: {player}, Local Player: {runner.LocalPlayer}");

        // In Shared Mode, each client spawns only its own local player object.
        if (player != runner.LocalPlayer)
            return;

        if (!playerPrefab.IsValid)
        {
            Debug.LogError("Player prefab is invalid!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition(player);

        Debug.Log($"[SPAWN] Spawning local player {player} at {spawnPos}");

        NetworkObject spawned = runner.Spawn(
            playerPrefab,
            spawnPos,
            Quaternion.identity,
            player
        );

        if (spawned != null)
        {
            runner.SetPlayerObject(player, spawned);
            spawned.gameObject.name = $"NetworkPlayer_Local_{player.PlayerId}";

            Debug.Log($"✅ Spawn succeeded for local player {player}");
            Debug.Log($"Input Authority: {spawned.InputAuthority}");
            Debug.Log($"State Authority: {spawned.StateAuthority}");
        }
        else
        {
            Debug.LogError($"❌ Spawn failed for local player {player}");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[LEFT] Player left: {player}");

        NetworkObject playerObject = runner.GetPlayerObject(player);

        if (playerObject != null && playerObject.HasStateAuthority)
        {
            runner.Despawn(playerObject);
        }
    }

    private Vector3 GetSpawnPosition(PlayerRef player)
    {
        int id = player.PlayerId;

        return new Vector3(
            id * 2f,
            2f,
            5f
        );
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {}

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}

    public void OnConnectedToServer(NetworkRunner runner) {}

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) {}

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}

    public void OnSceneLoadDone(NetworkRunner runner) {}

    public void OnSceneLoadStart(NetworkRunner runner) {}

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
}