using System;
using System.Collections;
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

    [Header("Scenes")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string battleSceneName = "NetworkScene";

    private NetworkRunner runner;
    private NetworkedXRAvatar localAvatar;
    public PlayerHealth LocalPlayerHealth { get; private set; }

    [SerializeField] private int minimumPlayersToStart = 2;
    [SerializeField] private int maxPlayers = 4;
    private int cachedRoom1Players;
    private int cachedRoom2Players;
    private bool roomCountReady;

    public int CachedRoom1Players => cachedRoom1Players;
    public int CachedRoom2Players => cachedRoom2Players;
    public bool RoomCountReady => roomCountReady;
    
    
    private bool loadBattleRequested;
    private bool checkLobbyRequested;
    private bool spawnLocalPlayer;
    private bool joiningRoom;

    public event Action<int, int> LobbyStatusChanged;
    public event Action<int, int> RoomCountChanged;
    public event Action<string> RoomJoinFailed;

    private const string Room1Name = "Room1";
    private const string Room2Name = "Room2";

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

        CreateRunnerIfNotExists();

        var result = await runner.JoinSessionLobby(SessionLobby.Shared);

        if (result.Ok)
        {
            Debug.Log($"(NetworkManager): Joined session lobby.");
        }
        else
        {
            Debug.LogError($"(NetworkManager): Failed to join session lobby! : {result.ErrorMessage}");
        }
    }

    private void Update()
    {
        if (!checkLobbyRequested) return;

        checkLobbyRequested = false;
        
        CheckLobbyReady();
    }

    private void CreateRunnerIfNotExists()
    {
        if (runner) return;

        runner = Instantiate(networkRunnerPrefab);
        runner.name = "NetworkRunner";
        DontDestroyOnLoad(runner.gameObject);
        
        runner.AddCallbacks(this);
        runner.ProvideInput = true;
    }

    public void JoinRoom1()
    {
        JoinRoom(Room1Name);
    }

    public void JoinRoom2()
    {
        JoinRoom(Room2Name);
    }

    public async void JoinRoom(string roomName)
    {
        if (joiningRoom) return;
        
        CreateRunnerIfNotExists();

        joiningRoom = true;

        Debug.Log($"(NetworkManager): Joining room {roomName}...");

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            PlayerCount = maxPlayers,
            IsOpen = true,
            IsVisible = true,
            Scene = SceneRef.FromIndex(1),
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });

        joiningRoom = false;

        if (result.Ok)
        {
            Debug.Log($"(NetworkManager): Successfully joined room {roomName}!");
        }
        else
        {
            Debug.LogError($"(NetworkManager): Failed to join room {roomName}! : {result.ErrorMessage}");
            
            RoomJoinFailed?.Invoke(result.ErrorMessage);
        }
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

    public void CheckLobbyStatus()
    {
        if (runner == null || !runner.IsRunning)
        {
            LobbyStatusChanged?.Invoke(0, 0);
            return;
        }

        var playerCount = 0;
        var readyCount = 0;

        foreach (var player in runner.ActivePlayers)
        {
            playerCount++;
            
            if (!runner.TryGetPlayerObject(player, out var playerObject)) continue;

            if (!playerObject.TryGetComponent(out NetworkedXRAvatar avatar)) continue;

            if (avatar.IsReady) readyCount++;
        }
        
        LobbyStatusChanged?.Invoke(playerCount, readyCount);
    }

    public void RequestLobbyCheck()
    {
        checkLobbyRequested = true;
    }

    public void CheckLobbyReady()
    {
        if (runner == null || !runner.IsRunning || !runner.IsSceneAuthority || loadBattleRequested) return;

        if (SceneManager.GetActiveScene().name != lobbySceneName) return;

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

        runner.LoadScene(SceneRef.FromIndex(2), LoadSceneMode.Single);
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

        runner.LoadScene(SceneRef.FromIndex(2), LoadSceneMode.Single);
    }

    private IEnumerator SetupArenaDelay()
    {
        while (!PlayerSpawns.Instance) yield return null;
        SetupArena();
    }

    private void SetupArena()
    {
        if (!PlayerSpawns.Instance)
        {
            Debug.LogError("(NetworkManager): Cannot setup arena, no PlayerSpawns instance found.");
            return;
        }

        var playerCount = runner.ActivePlayers.Count();
        
        PlayerSpawns.Instance.SetActivePlayerCount(playerCount);
        
        PositionPlayer();
    }

    private int GetLocalPlayerSlot()
    {
        if (!runner) return -1;

        var players = runner.ActivePlayers.OrderBy(player => player.PlayerId).ToList();

        return players.FindIndex(player => player == runner.LocalPlayer);
    }

    private void TrySpawnLocalPlayer()
    {
        if (!spawnLocalPlayer) return;

        if (!runner || !runner.IsRunning) return;

        if (localAvatar) return;

        var activeSceneName = SceneManager.GetActiveScene().name;

        if (activeSceneName != lobbySceneName && activeSceneName != battleSceneName) return;

        if (!playerPrefab.IsValid)
        {
            Debug.LogError("(NetworkManager): Could not find player prefab.");
            return;
        }

        var player = runner.LocalPlayer;

        var spawnPos = GetSpawnPosition(player);
        
        var spawned = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);

        if (!spawned)
        {
            Debug.LogError($"(NetworkManager): Spawn failed for local player {player}!");
            return;
        }
        
        runner.MakeDontDestroyOnLoad(spawned.gameObject);
        runner.SetPlayerObject(player, spawned);

        spawned.gameObject.name = $"NetworkPlayer_Local_{player.PlayerId}";
        
        localAvatar = spawned.GetComponent<NetworkedXRAvatar>();

        LocalPlayerHealth = spawned.GetComponent<PlayerHealth>();

        spawnLocalPlayer = false;
        
        CheckLobbyStatus();

        if (runner.IsSceneAuthority) RequestLobbyCheck();
    }

    private void PositionPlayer()
    {
        if (!PlayerSpawns.Instance)
        {
            Debug.LogError("(NetworkManager): Cannot position player, no PlayerSpawns instance found.");
            return;
        }

        var slotIndex = GetLocalPlayerSlot();

        if (slotIndex < 0)
        {
            Debug.LogError("(NetworkManager): Could not determine local player slot.");
            return;
        }

        var spawnPoint = PlayerSpawns.Instance.GetSpawnPoint(slotIndex);

        if (!spawnPoint)
        {
            Debug.LogError($"(NetworkManager): Could not find spawn point for slot {slotIndex}");
            return;
        }

        if (!XRReferences.Instance)
        {
            Debug.LogError("(NetworkManager): Could not find XRReferences instance.");
            return;
        }

        var xrOrigin = XRReferences.Instance.GetComponent<Unity.XR.CoreUtils.XROrigin>();

        if (!xrOrigin)
        {
            Debug.LogError("(NetworkManager): Could not find XR Origin.");
            return;
        }

        xrOrigin.MatchOriginUpCameraForward(spawnPoint.up, spawnPoint.forward);
        xrOrigin.MoveCameraToWorldLocation(spawnPoint.position);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[JOIN] Player joined: {player}, Local Player: {runner.LocalPlayer}");

        // In Shared Mode, each client spawns only its own local player object.
        if (player != runner.LocalPlayer)
        {
            CheckLobbyStatus();
            
            if (runner.IsSceneAuthority) RequestLobbyCheck();
            return;
        }

        spawnLocalPlayer = true;
        TrySpawnLocalPlayer();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[LEFT] Player left: {player}");

        var playerObject = runner.GetPlayerObject(player);

        if (playerObject != null && playerObject.HasStateAuthority)
        {
            runner.Despawn(playerObject);
        }
        
        CheckLobbyStatus();
        
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

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        var room1Players = 0;
        var room2Players = 0;

        foreach (var session in sessionList)
        {
            if (session.Name == Room1Name) room1Players = session.PlayerCount;
            if (session.Name == Room2Name) room2Players = session.PlayerCount;
        }

        cachedRoom1Players = room1Players;
        cachedRoom2Players = room2Players;
        roomCountReady = true;
        
        RoomCountChanged?.Invoke(room1Players, room2Players);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        localAvatar = null;
        LocalPlayerHealth = null;
        
        loadBattleRequested = false;
        checkLobbyRequested = false;
        spawnLocalPlayer = false;
        joiningRoom = false;
        
        if (this.runner == runner) this.runner = null;
        
        LobbyStatusChanged?.Invoke(0, 0);
        RoomCountChanged?.Invoke(0, 0);
    }

    public void OnConnectedToServer(NetworkRunner runner) {}

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) {}

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        loadBattleRequested = false;

        TrySpawnLocalPlayer();
        
        if (SceneManager.GetActiveScene().name == battleSceneName)
        {
            StartCoroutine(SetupArenaDelay());
            
            LocalPlayerHealth?.ResetHealth();
        }
        
        CheckLobbyStatus();
        
        if (runner.IsSceneAuthority) RequestLobbyCheck();
    }

    public void OnSceneLoadStart(NetworkRunner runner) {}

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
}