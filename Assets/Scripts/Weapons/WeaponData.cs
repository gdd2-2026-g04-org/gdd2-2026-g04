using UnityEngine;

namespace GameAssets.Weapons
{
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "GameAssets/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "Sword";

        [Header("Damage")]
        public int damage = 10;

        [Header("Thresholds")]
        public float minSpeedForTrail = 15f;
        public float minSpeedForSound = 15f;

        [Header("Sound Cooldown")]
        public float soundCooldown = 0.4f;
    }
}
