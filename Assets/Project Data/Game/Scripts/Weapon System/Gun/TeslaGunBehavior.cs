using System;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class TeslaGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] GameObject lightningLoopParticle;
        [SerializeField] float chargeDuration;
        [SerializeField] DuoInt targetsHitGoal;
        [SerializeField] float stunDuration = 0.2f;
        TweenCase _shootTweenCase;
        bool _isCharging;
        bool _isCharged;
        bool _isChargeParticleActivated;
        float _fullChargeTime;
        TeslaGunUpgrade _upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);
            _upgrade = UpgradesController.Get<TeslaGunUpgrade>(data.UpgradeType);
            var bulletObj = _upgrade.BulletPrefab;
            _bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));
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
            _bulletSpeed = stage.BulletSpeed;
        }

        bool NotCharged => !_isCharging && !_isCharged;
        bool Charged => _fullChargeTime >= Time.timeSinceLevelLoad;

        void Update()
        {
            if (NoTarget)
            {
                if (_isCharging || _isCharged) CancelCharge();
                return;
            }

            if (NotCharged)
            {
                _isCharging = true;
                _isChargeParticleActivated = false;
                _fullChargeTime = Time.timeSinceLevelLoad + (chargeDuration / AtkSpdMult);
            }

            if (Charged)
            {
                //start charge particle 0.5 sec before charge complete
                if (!_isChargeParticleActivated && _fullChargeTime - Time.timeSinceLevelLoad <= 0.5f)
                {
                    _isChargeParticleActivated = true;
                    shootParticleSystem.Play();
                }

                if (VisionIsClear && !NotLookAtTarget) CharacterBehaviour.FocusOnTarget();
                else CharacterBehaviour.TargetUnreachable();
                return;
            }

            // activate loop particle once charged
            if (!_isCharged)
            {
                _isCharged = true;
                lightningLoopParticle.SetActive(true);
            }
        }

        public override void Shoot()
        {
            PlayShootAnimation();

            for (var k = 0; k < BulletsNumber; k++)
            {
                var settings = PoolSettings();
                var bullet = _bulletPool.GetPlayerBullet(settings);
                if (bullet is TeslaBulletBehavior tesla)
                {
                    tesla.Initialise(Damage, BulletSpeed, Target, bulletLifeTime, false, stunDuration);
                    tesla.SetHitsAmount(HitsAmount);
                }
            }

            CancelCharge();
        }

        int HitsAmount => targetsHitGoal.Random();
        int BulletsNumber => RandomBulletsAmount(_upgrade);

        void PlayShootAnimation()
        {
            _shootTweenCase.KillActive();
            _shootTweenCase = transform.DOLocalMoveZ(-0.15f, chargeDuration / _attackDelay * 0.3f).OnComplete(delegate { _shootTweenCase = transform.DOLocalMoveZ(0, chargeDuration / _attackDelay * 0.6f); });
            if (shootParticleSystem) shootParticleSystem.Play();
            CharacterBehaviour.FocusOnTarget();
            CharacterBehaviour.OnGunShooted();
            AudioController.Play(AudioController.Sounds.shotTesla, volumePercentage: 0.8f);
            CharacterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);
        }

        void CancelCharge()
        {
            _isCharging = false;
            _isCharged = false;
            _isChargeParticleActivated = false;
            lightningLoopParticle.SetActive(false);
            shootParticleSystem.Stop();
        }

        void OnDrawGizmos()
        {
            if (CharacterBehaviour == null)
                return;
            if (CharacterBehaviour.ClosestEnemyBehaviour == null)
                return;
            var defCol = Gizmos.color;
            Gizmos.color = Color.red;
            var shootDirection =
                CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) -
                shootPoint.position;
            Gizmos.DrawLine(shootPoint.position - shootDirection.normalized * 1.5f,
                CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y));
            Gizmos.color = defCol;
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.TeslaHolderTransform);
            transform.ResetLocal();
        }
    }
}