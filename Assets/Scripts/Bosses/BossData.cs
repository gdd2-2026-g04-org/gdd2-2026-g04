using UnityEngine;

namespace GameAssets.Health
{
    [CreateAssetMenu(fileName = "NewBoss", menuName = "GameAssets/Boss Data")]
    public class BossData : ScriptableObject
    {
        [Header("Identity")]
        public string bossName = "Boss";

        [Header("Health")]
        public int maxHP = 200;
    }
}
