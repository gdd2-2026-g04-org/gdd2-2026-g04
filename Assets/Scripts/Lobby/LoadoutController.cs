using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadoutController : MonoBehaviour
{
    [Header("Equipment")]
    [SerializeField] private GameObject warriorSword;
    [SerializeField] private GameObject warriorShield;
    [SerializeField] private GameObject mageStaff;
    [SerializeField] private GameObject healerStaff;
    [SerializeField] private GameObject healerBook; 
    [SerializeField] private GameObject archerBow;

    [Header("Scene")] [SerializeField] private string battleSceneName = "NetworkScene";

    [SerializeField] private PlayerClass selectedClass = PlayerClass.None;
    private bool isBattleScene;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Start()
    {
        if (LocalClassSelector.Instance)
        {
            LocalClassSelector.Instance.ClassChanged += OnClassChanged;
            selectedClass = LocalClassSelector.Instance.SelectedClass;
        }
        
        UpdateSceneState(SceneManager.GetActiveScene());
        ApplyLoadout();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        if (LocalClassSelector.Instance) LocalClassSelector.Instance.ClassChanged -= OnClassChanged;
    }

    private void OnClassChanged(PlayerClass playerClass)
    {
        selectedClass = playerClass;
        ApplyLoadout();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneState(scene);
        ApplyLoadout();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        SetActive(warriorSword, false);
        SetActive(warriorShield, false);
        SetActive(mageStaff, false);
        SetActive(healerStaff, false);
        SetActive(healerBook, false);
        SetActive(archerBow, false);
    }

    private void UpdateSceneState(Scene scene)
    {
        isBattleScene = scene.name == battleSceneName;
    }
    
    private void ApplyLoadout()
    {
        bool isHealer = selectedClass == PlayerClass.Healer;

        SetActive(warriorSword, isBattleScene && selectedClass == PlayerClass.Warrior);
        SetActive(warriorShield, isBattleScene && selectedClass == PlayerClass.Warrior);
        SetActive(mageStaff, isBattleScene && selectedClass == PlayerClass.Mage);
        SetActive(healerStaff, isBattleScene && isHealer);
        SetActive(healerBook, isBattleScene && isHealer); // Left hand book activated
        SetActive(archerBow, isBattleScene && selectedClass == PlayerClass.Archer);
    }
    
    private static void SetActive(GameObject target, bool active)
    {
        if (target && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }
}