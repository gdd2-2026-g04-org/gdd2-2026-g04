using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    private void Awake()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true);
    }

    public void HideGameOverScreen()
    {
        gameOverPanel.SetActive(false);
    }

    public void RestartGame()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError($"(GameOverUI): Could not find NetworkManager!");
            return;
        }
        
        NetworkManager.Instance.RequestBattleRestart();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
