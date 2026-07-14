using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTable : MonoBehaviour
{
    [SerializeField] private Image tutorialImage;
    [SerializeField] private Sprite warriorTutorial;
    [SerializeField] private Sprite healerTutorial;
    [SerializeField] private Sprite mageTutorial;
    [SerializeField] private Sprite archerTutorial;
    
    void Start()
    {
        if (LocalClassSelector.Instance)
        {
            LocalClassSelector.Instance.ClassChanged += UpdateTutorialImage;
        }
    }

    private void OnDisable()
    {
        if (LocalClassSelector.Instance) 
        {
            LocalClassSelector.Instance.ClassChanged -= UpdateTutorialImage;
        }
    }

    private void ToggleTutorial(bool b)
    {
        tutorialImage.gameObject.SetActive(b);
    }

    private void UpdateTutorialImage(PlayerClass c)
    {
        ToggleTutorial(c != PlayerClass.None);
        
        switch (c)
        {
            case PlayerClass.Warrior:
                tutorialImage.sprite = warriorTutorial;
                break;
            case PlayerClass.Healer:
                tutorialImage.sprite = healerTutorial;
                break;
            case PlayerClass.Mage:
                tutorialImage.sprite = mageTutorial;
                break;
            case PlayerClass.Archer:
                tutorialImage.sprite = archerTutorial;
                break;
            case PlayerClass.None:
                tutorialImage.gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(c), c, null);
        }
    }
}
