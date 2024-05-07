﻿using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{

    [System.Serializable]
    public class EnemyStats
    {
        [SerializeField] float hp;
        int calculatedHp;
        public float Hp => calculatedHp * difficulty.HealthMult;

        [SerializeField] float visionRange;
        public float VisionRange => visionRange;

        [Space]
        [SerializeField] float attackDistance;
        public float AttackDistance => attackDistance;
        [SerializeField] float fleeDistance;
        public float FleeDistance => fleeDistance;

        [SerializeField] DuoInt damage;
        DuoInt calculatedDamage;

        public DuoInt Damage => calculatedDamage * difficulty.DamageMult;

        [SerializeField] float aimDuration;
        public float AimDuration => aimDuration;

        [Header("Movement")]
        [SerializeField] float moveSpeed;
        public float MoveSpeed => moveSpeed;

        [SerializeField] float patrollingSpeed;
        public float PatrollingSpeed => patrollingSpeed;

        [SerializeField] float patrollingMutliplier;
        public float PatrollingMutliplier => patrollingMutliplier;

        [SerializeField] float patrollingIdleDuration;
        public float PatrollingIdleDuration => patrollingIdleDuration;

        [SerializeField] float angularSpeed;
        public float AngularSpeed => angularSpeed;

        [SerializeField] float preferedDistanceToPlayer;
        public float PreferedDistanceToPlayer => preferedDistanceToPlayer;

        [Header("Elite")]
        [SerializeField] int level;
        public float Level => level;
        [SerializeField] float eliteHealthMult;
        public float EliteHealthMult => eliteHealthMult;
        [SerializeField] float eliteDamageMult;
        public float EliteDamageMult => eliteDamageMult;


        [Header("Other")]
        [SerializeField] DuoInt healForPlayer;

        DuoInt calculatedHpForPlayer;
        public DuoInt HpForPlayer => calculatedHpForPlayer * difficulty.RestoredHpMult;

        [Space(5)]
        [SerializeField] float targetRingSize = 1.0f;
        public float TargetRingSize => targetRingSize;
        [SerializeField] float hitTextOffsetY = 17;
        public float HitTextOffsetY => hitTextOffsetY;
        [SerializeField] float hitTextOffsetForward = 0;
        public float HitTextOffsetForward => hitTextOffsetForward;

        // dynamic stats system
        float enemyDmgToPlayerHp; // how much enemy needs to hit to kill player
        List<HpToWeaponRelation> enemyHpToCreatureDmgRelations; // how much times player needs to hit for kill | value for each weapon
        float restoredHpToDamage; // how much hits do we restore
        float creatureDamage;

        DifficultySettings difficulty;

        public void InitialiseStatsRelation(int baseCreatureHealth)
        {
            enemyDmgToPlayerHp = (float)baseCreatureHealth / damage.Lerp(0.5f);

            enemyHpToCreatureDmgRelations = new List<HpToWeaponRelation>();

            for (var i = 0; i < WeaponsController.Database.Weapons.Length; i++)
            {
                var weaponUpg = UpgradesController.GetUpgrade<BaseUpgrade>(WeaponsController.Database.Weapons[i].UpgradeType) as BaseWeaponUpgrade;
                var firstStage = weaponUpg.Upgrades[1] as BaseWeaponUpgradeStage; // 0 stage is for locked weapon - has no stats

                var relation = new HpToWeaponRelation(WeaponsController.Database.Weapons[i].Type, hp / firstStage.Damage.Lerp(0.5f));
                enemyHpToCreatureDmgRelations.Add(relation);
            }

            restoredHpToDamage = (float)healForPlayer.Lerp(0.5f) / damage.Lerp(0.5f);
        }

        // creature damage is currently not used - actual damage is calculated based on relation to hp on the first level
        public void SetCurrentCreatureStats(int characterHealth, int weaponDmg, DifficultySettings difficulty)
        {
            this.creatureDamage = weaponDmg;
            this.difficulty = difficulty;

            var currentWeapon = WeaponsController.GetCurrentWeapon();
            var relation = enemyHpToCreatureDmgRelations.Find(r => r.weapon.Equals(currentWeapon.Type));

            calculatedHp = (int)(creatureDamage * relation.enemyHpToCreatureDmg);

            var dmgMid = characterHealth / enemyDmgToPlayerHp;
            var damageSpreadUp = (float)damage.secondValue / (float)damage.Lerp(0.5f);
            var damageSpreadDown = (float)damage.firstValue / (float)damage.Lerp(0.5f);
            calculatedDamage = new DuoInt((int)(dmgMid * damageSpreadDown), (int)(dmgMid * damageSpreadUp));

            var restoredHpMid = dmgMid * restoredHpToDamage;
            var hpSpreadUp = (float)healForPlayer.secondValue / (float)healForPlayer.Lerp(0.5f);
            var hpSpreadDown = (float)healForPlayer.firstValue / (float)healForPlayer.Lerp(0.5f);
            calculatedHpForPlayer = new DuoInt((int)(restoredHpMid * hpSpreadDown), (int)(restoredHpMid * hpSpreadUp));
        }

        [System.Serializable]
        class HpToWeaponRelation
        {
            public WeaponType weapon;
            public float enemyHpToCreatureDmg;  // how much times player needs to hit for kill

            public HpToWeaponRelation(WeaponType type, float relation)
            {
                weapon = type;
                enemyHpToCreatureDmg = relation;
            }
        }
    }

    public enum EnemyTier
    {
        Regular = 0,
        Elite = 1,
        Boss = 2,
    }
}