using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject defeatScreen;
    [SerializeField] private CanvasGroup gameOverGroup;

    [SerializeField] private float fadeDuration = 1.5f;

    private Coroutine fadeCoroutine;
    private void Awake()
    {
        HideGameOverScreen();
    }

    public void ShowGameOver(bool victory)
    {
        if (defeatScreen) defeatScreen.SetActive(!victory);
        if (victoryScreen) victoryScreen.SetActive(victory);
        
        if (gameOverPanel) gameOverPanel.SetActive(false);
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeGameOver());
    }

    private IEnumerator FadeGameOver()
    {
        if (!gameOverGroup) yield break;

        gameOverGroup.alpha = 0f;

        var timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            var t = timer / fadeDuration;
            gameOverGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        gameOverGroup.alpha = 1f;
        fadeCoroutine = null;
    }

    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }

    public void HideGameOverScreen()
    {
        gameOverPanel.SetActive(false);
        victoryScreen.SetActive(false);
        defeatScreen.SetActive(false);

        if (gameOverGroup) gameOverGroup.alpha = 0f;
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    public void ReturnToMainMenu()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError($"(GameOverUI): Could not find NetworkManager!");
            return;
        }
        
        NetworkManager.Instance.ReturnToMainMenu();
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
