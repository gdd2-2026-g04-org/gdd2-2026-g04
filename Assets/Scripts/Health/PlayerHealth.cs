using GameAssets.Player;
using UnityEngine;

namespace GameAssets.Health
{
    public class PlayerHealth : HealthComponent
    {
        [SerializeField] private ClassData classData;

        public ClassData Class => classData;
        public int Damage { get; private set; }

        protected override void Awake()
        {
            if (classData != null)
            {
                maxHP  = classData.baseHP;
                Damage = classData.baseDamage;
            }
            base.Awake();
        }

        private void Start()
        {
            FindFirstObjectByType<HealthSystemManager>()?.RegisterPlayer(this);
        }
    }
}
