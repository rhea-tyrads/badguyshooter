using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class MiniGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] Transform barrelTransform;
        [SerializeField] float fireRotationSpeed;
        float _spread;
        MinigunUpgrade _upgrade;
        TweenCase _shootAnim;


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
            barrelTransform.Rotate(Vector3.forward * fireRotationSpeed);
        }

        int BulletsNumber => RandomBulletsAmount(_upgrade);
        public override void GunUpdate()
        {
            if (NoTarget) return;
            RotateBarrelAnimation();
            if (NotReady) return;

            _shootDirection = AimAtTarget();
            if (OutOfAngle) return;

            if (TargetInSight)
            {
                PlayShootAnimation();
                _nextShootTime = FireRate();

                for (var k = 0; k < BulletsNumber; k++)
                {
                    foreach (var streamAngle in bulletStreamAngles)
                    {
                        var angle = Vector3.up * (Random.Range(-_spread, _spread) + streamAngle);
                        var settings = new PooledObjectSettings()
                            .SetPosition(shootPoint.position)
                            .SetRotation(CharacterBehaviour.transform.eulerAngles + angle);
                        
                        var bullet = _bulletPool.GetPlayerBullet(settings);
                        bullet.Initialise(Damage, BulletSpeed, Target, bulletLifeTime);
                    }
                }
            }
            else
            {
                TargetUnreachable();
            }
        }

        void PlayShootAnimation()
        {
            _shootAnim.KillActive();
            _shootAnim = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f)
                .OnComplete(delegate { _shootAnim = transform.DOLocalMoveZ(0, _attackDelay * 0.6f); });
            if (shootParticleSystem)  shootParticleSystem.Play();
            CharacterBehaviour.FocusOnTarget();
            CharacterBehaviour.OnGunShooted();
            AudioController.Play(AudioController.Sounds.shotMinigun);
        }

        public override void OnGunUnloaded()
        {
 
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.MinigunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            _bulletPool.ReturnToPoolEverything();
        }
    }
}