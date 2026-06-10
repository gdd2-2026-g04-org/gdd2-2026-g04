using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        var currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
