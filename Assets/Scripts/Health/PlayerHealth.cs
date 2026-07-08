using System;
using Fusion;
using GameAssets.Player;
using UnityEngine;

namespace GameAssets.Health
{
    public class PlayerHealth : HealthComponent
    {
        public static event Action<PlayerHealth> PlayerSpawned;
        public static event Action<PlayerHealth> PlayerDespawned;

        [Header("Class Data")]
        [SerializeField] private ClassData warriorData;
        [SerializeField] private ClassData mageData;
        [SerializeField] private ClassData healerData;
        [SerializeField] private ClassData archerData;
        
        [Networked]
        public int Damage { get; private set; }
        
        [Networked]
        public PlayerClass CurrentClass { get; private set; }

        private Shield localShield;

        public override void Spawned()
        {
            base.Spawned();
            
            PlayerSpawned?.Invoke(this);

            if (!Object.HasStateAuthority) return;

            GetLocalShield();
            
            var avatar = GetComponent<NetworkedXRAvatar>();
            if (avatar != null && avatar.SelectedClass != PlayerClass.None)
            {
                SetClass(avatar.SelectedClass);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            PlayerDespawned?.Invoke(this);
            base.Despawned(runner, hasState);
        }

        public void SetClass(PlayerClass playerClass)
        {
            if (!Object.HasStateAuthority) return;

            var data = GetClassData(playerClass);

            if (data == null)
            {
                Debug.LogError($"{name}: {playerClass} has no class data");
                return;
            }
            
            CurrentClass = playerClass;
            Damage = data.baseDamage;
            
            SetMaxHP(data.baseHP, refillHP: true);
        }

        public void RequestDamage(int damage)
        {
            if (damage <= 0) return;

            if (Object.HasStateAuthority)
            {
                ApplyDamagePlayer(damage);
                return;
            }

            RPC_RequestDamage(damage);
        }

        public void RequestHeal(int heal)
        {
            if (heal <= 0) return;

            if (Object.HasStateAuthority)
            {
                ApplyHealing(heal);
                return;
            }
            
            RPC_RequestHeal(heal);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestDamage(int damage)
        {
            ApplyDamagePlayer(damage);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestHeal(int heal)
        {
            ApplyHealing(heal);
        }

        private void ApplyDamagePlayer(int damage)
        {
            if (!Object.HasStateAuthority) return;
            
            GetLocalShield();

            if (localShield != null && localShield.isHeld && localShield.isRaised)
            {
                damage = Mathf.RoundToInt(damage * 0.5f);
                
                Debug.Log($"{name}: Shield reduced damage to {damage}!");
            }

            var damaged = ApplyDamage(damage);

            if (damaged && !IsAlive)
            {
                Debug.Log($"{name}: Player died!");
            }
        }

        private void GetLocalShield()
        {
            if (localShield != null) return;
            
            if (!Object.HasStateAuthority) return;

            if (XRReferences.Instance == null) return;

            localShield = XRReferences.Instance.GetComponentInChildren<Shield>(true);
        }


        private ClassData GetClassData(PlayerClass playerClass)
        {
            return playerClass switch
            {
                PlayerClass.Warrior => warriorData,
                PlayerClass.Mage => mageData,
                PlayerClass.Healer => healerData,
                PlayerClass.Archer => archerData,
                _ => null
            };
        }
    }
}
