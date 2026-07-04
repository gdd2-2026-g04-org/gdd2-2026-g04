using UnityEngine;
using Fusion;

public class LobbyUI : MonoBehaviour
{
    public void SelectWarriorClass()
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null) return;

        NetworkObject localPlayerObj = runner.GetPlayerObject(runner.LocalPlayer);
        if (localPlayerObj != null && localPlayerObj.TryGetComponent<NetworkedXRAvatar>(out var avatar))
        {
            avatar.SelectedClass = PlayerClass.Warrior;
            Debug.Log("Warrior class selected.");
        }
    }

    public void ToggleReadyState()
    {
        NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        if (runner == null) return;

        NetworkObject localPlayerObj = runner.GetPlayerObject(runner.LocalPlayer);
        if (localPlayerObj != null && localPlayerObj.TryGetComponent<NetworkedXRAvatar>(out var avatar))
        {
            avatar.IsReady = !avatar.IsReady;
            Debug.Log($"Ready state set to: {avatar.IsReady}");
            
            CheckAllPlayersReady(runner);
        }
    }

    private void CheckAllPlayersReady(NetworkRunner runner)
    {
        bool allReady = true;
        int activePlayersCount = 0;

        foreach (var player in runner.ActivePlayers)
        {
            NetworkObject pObj = runner.GetPlayerObject(player);
            if (pObj != null && pObj.TryGetComponent<NetworkedXRAvatar>(out var avatar))
            {
                activePlayersCount++;
                if (!avatar.IsReady)
                {
                    allReady = false;
                }
            }
            else
            {
                allReady = false;
            }
        }

        if (allReady && activePlayersCount > 0)
        {
            Debug.Log("All players ready! Transitioning to Battle Scene...");
            LoadBattleScene(runner);
        }
    }

    private void LoadBattleScene(NetworkRunner runner)
    {
        // Fusion 2.0 uses LoadScene to load scenes across the network.
        // Change "SampleScene" to the exact name of your battle scene.
        runner.LoadScene("SampleScene"); 
    }
}