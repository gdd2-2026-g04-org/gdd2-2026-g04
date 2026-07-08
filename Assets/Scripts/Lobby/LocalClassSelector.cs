using System;
using UnityEngine;

public class LocalClassSelector : MonoBehaviour
{
    public static LocalClassSelector Instance { get; private set; }

    public PlayerClass SelectedClass { get; private set; } = PlayerClass.None;

    public event Action<PlayerClass> ClassChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool SelectClass(PlayerClass selectedClass)
    {
        if (selectedClass == PlayerClass.None)
        {
            Debug.LogWarning("Cannot select \"None\" as class.");
            return false;
        }

        if (SelectedClass == selectedClass) return false;

        SelectedClass = selectedClass;
        ClassChanged?.Invoke(SelectedClass);
        
        Debug.Log($"Locally selected class: {SelectedClass}");
        return true;
    }

    public void ClearSelection()
    {
        if (SelectedClass == PlayerClass.None) return;
        
        SelectedClass = PlayerClass.None;
        ClassChanged?.Invoke(SelectedClass);
    }
}
