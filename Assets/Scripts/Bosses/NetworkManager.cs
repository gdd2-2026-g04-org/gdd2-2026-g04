using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using GameAssets.Health;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }
    
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private NetworkRunner runner;
    private NetworkedXRAvatar localAvatar;
    public PlayerHealth LocalPlayerHealth { get; private set; }

    [SerializeField] private int minimumPlayersToStart = 2;
    private bool loadBattleRequested;
    private bool checkLobbyRequested;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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

    private void Update()
    {
        if (!checkLobbyRequested) return;

        checkLobbyRequested = false;
        
        CheckLobbyReady();
    }

    public void SetLocalReady(bool ready)
    {
        if (localAvatar == null)
        {
            Debug.LogWarning("Can't set local ready, local avatar has not spawned!");
            return;
        }
        
        localAvatar.SetReady(ready);
    }

    public void RequestLobbyCheck()
    {
        checkLobbyRequested = true;
    }

    public void CheckLobbyReady()
    {
        if (runner == null || !runner.IsRunning || !runner.IsSceneAuthority || loadBattleRequested) return;

        if (SceneManager.GetActiveScene().name != "LobbyScene") return;

        var playerCount = 0;

        foreach (var player in runner.ActivePlayers)
        {
            playerCount++;

            if (!runner.TryGetPlayerObject(player, out var playerObject)) return;

            if (!playerObject.TryGetComponent(out NetworkedXRAvatar avatar)) return;

            if (avatar.SelectedClass == PlayerClass.None || !avatar.IsReady) return;
        }

        if (playerCount < minimumPlayersToStart)
        {
            Debug.Log($"Waiting for players: {playerCount}/{minimumPlayersToStart}");
            return;
        }
        
        LoadBattleScene();
    }

    private void LoadBattleScene()
    {
        if (loadBattleRequested) return;

        if (runner == null || !runner.IsRunning || !runner.IsSceneAuthority) return;
        loadBattleRequested = true;
        
        Debug.Log($"All {runner.ActivePlayers.Count()} connected players " + "are ready. Loading battle scene.");

        runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
    }
    
    public void RequestBattleRestart()
    {
        if (!localAvatar)
        {
            Debug.LogWarning("(NetworkManager): Cannot restart because local avatar is unavailable.");
            return;
        }

        localAvatar.RequestBattleRestart();
    }

    public void RestartBattle()
    {
        if (!runner || !runner.IsRunning || !runner.IsSceneAuthority || loadBattleRequested) return;

        loadBattleRequested = true;

        runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[JOIN] Player joined: {player}, Local Player: {runner.LocalPlayer}");

        // In Shared Mode, each client spawns only its own local player object.
        if (player != runner.LocalPlayer)
        {
            if (runner.IsSceneAuthority) RequestLobbyCheck();
            return;
        }

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
            runner.MakeDontDestroyOnLoad(spawned.gameObject);
            runner.SetPlayerObject(player, spawned);
            spawned.gameObject.name = $"NetworkPlayer_Local_{player.PlayerId}";

            Debug.Log($"✅ Spawn succeeded for local player {player}");
            Debug.Log($"Input Authority: {spawned.InputAuthority}");
            Debug.Log($"State Authority: {spawned.StateAuthority}");
            
            localAvatar = spawned.GetComponent<NetworkedXRAvatar>();
            LocalPlayerHealth = spawned.GetComponent<PlayerHealth>();
            if (runner.IsSceneAuthority) RequestLobbyCheck();
        }
        else
        {
            Debug.LogError($"❌ Spawn failed for local player {player}");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[LEFT] Player left: {player}");

        var playerObject = runner.GetPlayerObject(player);

        if (playerObject != null && playerObject.HasStateAuthority)
        {
            runner.Despawn(playerObject);
        }
        
        if (runner.IsSceneAuthority) RequestLobbyCheck();
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

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        localAvatar = null;
        loadBattleRequested = false;
        checkLobbyRequested = false;

        if (this.runner == runner) this.runner = null;
    }

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

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        loadBattleRequested = false;
        
        if (SceneManager.GetActiveScene().name == "NetworkScene") LocalPlayerHealth?.ResetHealth();
    }

    public void OnSceneLoadStart(NetworkRunner runner) {}

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
}