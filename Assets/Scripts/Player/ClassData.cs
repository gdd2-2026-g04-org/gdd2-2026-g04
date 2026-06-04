using UnityEngine;

namespace GameAssets.Player
{
    [CreateAssetMenu(fileName = "NewClass", menuName = "GameAssets/Class Data")]
    public class ClassData : ScriptableObject
    {
        [Header("Identity")]
        public string className = "Warrior";

        [Header("Base Stats")]
        public int baseHP     = 150;
        public int baseDamage = 10;
    }
}
