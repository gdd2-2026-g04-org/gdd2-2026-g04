using UnityEngine;

public class MageMana : MonoBehaviour
{
    [Header("Mana Settings")]
    [SerializeField] private int maxMana = 120;
    [SerializeField] private int startingMana = 120;

    public int CurrentMana { get; private set; }
    public int MaxMana => maxMana;

    private void OnEnable()
    {
        CurrentMana = startingMana;
    }

    private void OnDisable() { }

    public bool HasMana(int amount) => CurrentMana >= amount;

    public bool TrySpend(int amount)
    {
        if (CurrentMana < amount) return false;
        CurrentMana -= amount;
        return true;
    }

    public void Restore(int amount)
    {
        CurrentMana = Mathf.Min(CurrentMana + amount, maxMana);
    }
}
