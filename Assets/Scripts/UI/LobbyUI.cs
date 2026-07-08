using System;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    private bool localReady;
    [SerializeField] private Button readyButton;

    [SerializeField] private TMP_Text playerCountText;

    private void Start()
    {
        if (!NetworkManager.Instance)
        {
            Debug.LogError("(LobbyUI): NetworkManager instance is null!");
            UpdatePlayerCount(0, 0);
            return;
        }

        NetworkManager.Instance.LobbyStatusChanged += UpdatePlayerCount;
        
        NetworkManager.Instance.CheckLobbyStatus();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.LobbyStatusChanged -= UpdatePlayerCount;
        }
    }

    public void SelectWarriorClass()
    {
        SelectClass(PlayerClass.Warrior);
    }

    public void SelectMageClass()
    {
        SelectClass(PlayerClass.Mage);
    }

    public void SelectHealerClass()
    {
        SelectClass(PlayerClass.Healer);
    }

    public void SelectArcherClass()
    {
        SelectClass(PlayerClass.Archer);
    }

    public void ToggleReadyState()
    {
        if (LocalClassSelector.Instance == null) return;

        if (LocalClassSelector.Instance.SelectedClass == PlayerClass.None)
        {
            Debug.LogWarning("Cannot ready up before selecting a class.");
            return;
        }

        if (NetworkManager.Instance == null)
        {
            Debug.LogError("NetworkManager is null!");
            return;
        }

        localReady = !localReady;
        NetworkManager.Instance.SetLocalReady(localReady);
        
        Debug.Log($"Local ready state: {localReady}");
    }
    
    

    private void SelectClass(PlayerClass selectedClass)
    {
        if (LocalClassSelector.Instance == null)
        {
            Debug.LogError("LocalClassSelector Instance is null!");
            return;
        }

        var changed = LocalClassSelector.Instance.SelectClass(selectedClass);

        if (changed && localReady)
        {
            localReady = false;
            NetworkManager.Instance?.SetLocalReady(false);
        }
    }

    private void UpdatePlayerCount(int playerCount, int readyCount)
    {
        if (playerCountText == null) return;
        
        playerCountText.text = $"Players: {playerCount}\nReady: {readyCount} / {playerCount}";
    }
}