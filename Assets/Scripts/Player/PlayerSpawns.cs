using System;
using UnityEngine;

public class PlayerSpawns : MonoBehaviour
{
    public static PlayerSpawns Instance { get; private set; }

    [Serializable]
    private class PlayerSlot
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject pedestal;

        public Transform SpawnPoint => spawnPoint;

        public void SetVisible(bool visible)
        {
            if (pedestal && pedestal.activeSelf != visible)
            {
                pedestal.SetActive(visible);
            }
        }
    }

    [SerializeField] private PlayerSlot[] slots;

    public int Count => slots?.Length ?? 0;

    private void Awake()
    {
        Instance = this;
        
        SetActivePlayerCount(0);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public Transform GetSpawnPoint(int slotIndex)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length) return null;

        return slots[slotIndex].SpawnPoint;
    }

    public void SetActivePlayerCount(int playerCount)
    {
        if (slots == null) return;
        
        playerCount = Mathf.Clamp(playerCount, 0, slots.Length);

        for (var i = 0; i < slots.Length; i++)
        {
            slots[i].SetVisible(i < playerCount);
        }
    }
}
