using System;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    private bool localReady;
    [SerializeField] private Button readyButton;

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
}