using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject roomsPanel;

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenRoomsPanel()
    {
        mainPanel.SetActive(false);
        roomsPanel.SetActive(true);
    }

    public void ReturnToMainPanel()
    {
        settingsPanel.SetActive(false);
        roomsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
