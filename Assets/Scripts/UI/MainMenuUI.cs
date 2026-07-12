using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject roomsPanel;

    [Header("Rooms")]
    [SerializeField] private Button room1Button;
    [SerializeField] private TMP_Text room1Text;
    [SerializeField] private Button room2Button;
    [SerializeField] private TMP_Text room2Text;

    private void Start()
    {
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.RoomCountChanged += UpdateRoomCount;
            NetworkManager.Instance.RoomJoinFailed += OnRoomJoinFailed;
        }
        
        UpdateRoomCount(0, 0);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenRoomsPanel()
    {
        mainPanel.SetActive(false);
        roomsPanel.SetActive(true);

        if (!NetworkManager.Instance)
        {
            ShowRoomCountUnknown();
            return;
        }

        if (NetworkManager.Instance.RoomCountReady)
        {
            UpdateRoomCount(NetworkManager.Instance.CachedRoom1Players, NetworkManager.Instance.CachedRoom2Players);
        }
        else
        {
            ShowRoomCountUnknown();
        }
    }

    public void ReturnToMainPanel()
    {
        settingsPanel.SetActive(false);
        roomsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    private void OnDisable()
    {
        if (NetworkManager.Instance)
        {
            NetworkManager.Instance.RoomCountChanged -= UpdateRoomCount;
            NetworkManager.Instance.RoomJoinFailed -= OnRoomJoinFailed;
        }
    }

    public void JoinRoom1()
    {
        ToggleButtonInteractable(false);
        NetworkManager.Instance?.JoinRoom1();
    }

    public void JoinRoom2()
    {
        ToggleButtonInteractable(false);
        NetworkManager.Instance?.JoinRoom2();
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowRoomCountUnknown()
    {
        if (room1Text) room1Text.text = "Room 1:\n? / 4 players";
        if (room2Text) room2Text.text = "Room 2:\n? / 4 players";
        
        ToggleButtonInteractable(false);
    }

    private void UpdateRoomCount(int room1Players, int room2Players)
    {
        if (room1Text) room1Text.text = $"Room 1:\n{room1Players} / 4 players";
        
        if (room2Text) room2Text.text = $"Room 2:\n{room2Players} / 4 players";

        if (room1Button) room1Button.interactable = room1Players < 4;
        if (room2Button) room2Button.interactable = room2Players < 4;
    }

    private void ToggleButtonInteractable(bool interactable)
    {
        if (room1Button) room1Button.interactable = interactable;
        
        if (room2Button) room2Button.interactable = interactable;
    }

    private void OnRoomJoinFailed(string error)
    {
        Debug.LogWarning($"(MainMenuUI): Room join failed: {error}");
        ToggleButtonInteractable(true);

        if (NetworkManager.Instance && NetworkManager.Instance.RoomCountReady)
        {
            UpdateRoomCount(NetworkManager.Instance.CachedRoom1Players, NetworkManager.Instance.CachedRoom2Players);
        }
    }
}
