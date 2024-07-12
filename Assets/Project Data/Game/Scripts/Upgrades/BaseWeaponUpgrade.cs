using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Watermelon.Upgrades
{
    [System.Serializable]
    public class BaseWeaponUpgrade : Upgrade<BaseWeaponUpgradeStage>
    {
        [Space(20)]
        public float DPS_DEFAULT;
        public float DPS_MAX;


        private void OnValidate()
        {
            DPS_DEFAULT = GET_DPS(FirstStage);
            DPS_MAX = GET_DPS(MaxStage);
        }

        float GET_DPS(BaseWeaponUpgradeStage stage)
        {
            var damage = stage.Damage;
            var fireRate = stage.FireRate;
            var bullets = stage.BulletsPerShot;
            return damage.Middle() * fireRate * bullets.Middle();
        }

        [Header("Prefabs")] [SerializeField] GameObject weaponPrefab;
        public GameObject WeaponPrefab => weaponPrefab;

        [SerializeField] GameObject bulletPrefab;
        public GameObject BulletPrefab => bulletPrefab;

        public override void Initialise()
        {
        }
    }
}