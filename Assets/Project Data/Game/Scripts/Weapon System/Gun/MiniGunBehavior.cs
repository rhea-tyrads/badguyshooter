using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Watermelon.SquadShooter
{
    public class MiniGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] Transform barrelTransform;
        [SerializeField] float fireRotationSpeed;
        MinigunUpgrade _upgrade;
     

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);
            _upgrade = UpgradesController.Get<MinigunUpgrade>(data.UpgradeType);
            var bulletPrefab = _upgrade.BulletPrefab;
            _bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 5, true));
            RecalculateDamage();
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            var stage = _upgrade.GetCurrentStage();
            damage = stage.Damage;
            var atkSpdMult = CharacterBehaviour.isAtkSpdBooster ? CharacterBehaviour.atkSpdBoosterMult : 1;
            _attackDelay = 1f / (stage.FireRate * atkSpdMult);
            _spread = stage.Spread;
            _bulletSpeed = stage.BulletSpeed;
        }

        void RotateBarrelAnimation()
        {
            barrelTransform.Rotate(Vector3.forward * (fireRotationSpeed * Time.deltaTime));
        }

        int BulletsNumber => RandomBulletsAmount(_upgrade);

        void Update()
        {
            if (NoTarget) return;
            RotateBarrelAnimation();
        }

        public override void Shoot()
        {
            PlayShootAnimation();
            for (var k = 0; k < BulletsNumber; k++)
            {
                SpawnBullet(k);
            }
        }

        void PlayShootAnimation()
        {
            _shootTweenCase.KillActive();
            _shootTweenCase = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f).OnComplete(delegate { _shootTweenCase = transform.DOLocalMoveZ(0, _attackDelay * 0.6f); });
            if (shootParticleSystem) shootParticleSystem.Play();
            CharacterBehaviour.FocusOnTarget();
            CharacterBehaviour.OnGunShooted();
            AudioController.Play(AudioController.Sounds.shotMinigun);
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.MinigunHolderTransform);
            transform.ResetLocal();
        }
    }
}