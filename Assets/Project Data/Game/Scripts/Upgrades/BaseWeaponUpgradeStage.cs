using UnityEngine;

namespace Watermelon.Upgrades
{
    [System.Serializable]
    public class BaseWeaponUpgradeStage : BaseUpgradeStage
    {
        [Header("Data")] [SerializeField] DuoInt damage;
        public DuoInt Damage => damage;

        [SerializeField] float rangeRadius;
        public float RangeRadius => rangeRadius;

        [SerializeField, Tooltip("Shots Per Second")]
        float fireRate;

        public float FireRate => fireRate;

        [SerializeField] float spread;
        public float Spread => spread;

        [SerializeField] int power;
        public int Power => power;

        [SerializeField] DuoInt bulletsPerShot = new(1, 1);
        public DuoInt BulletsPerShot => bulletsPerShot;

        [SerializeField] DuoFloat bulletSpeed;
        public DuoFloat BulletSpeed => bulletSpeed;

        // key upgrade - "ideal" way to play the game, based on this upgrades sequence is built economy
        [SerializeField] int keyUpgradeNumber = -1;
        public int KeyUpgradeNumber => keyUpgradeNumber;
    }
}