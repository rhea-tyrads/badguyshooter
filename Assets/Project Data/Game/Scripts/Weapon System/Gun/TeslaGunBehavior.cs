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

        public override void GunUpdate()
        {
            if (NoTarget)
            {
                if (_isCharging || _isCharged) CancelCharge();
                return;
            }

            var atkSpdMult = CharacterBehaviour.isAtkSpdBooster ? CharacterBehaviour.atkSpdBoosterMult : 1;

            // if not charging - start
            if (!_isCharging && !_isCharged)
            {
                _isCharging = true;
                _isChargeParticleActivated = false;
                _fullChargeTime = Time.timeSinceLevelLoad + (chargeDuration / atkSpdMult);
            }

            _shootDirection = AimAtTarget();

            // wait for full charge
            if (_fullChargeTime >= Time.timeSinceLevelLoad)
            {
                //start charge particle 0.5 sec before charge complete
                if (!_isChargeParticleActivated && _fullChargeTime - Time.timeSinceLevelLoad <= 0.5f)
                {
                    _isChargeParticleActivated = true;
                    shootParticleSystem.Play();
                }

                if (TargetInSight && !OutOfAngle) CharacterBehaviour.FocusOnTarget();
                else CharacterBehaviour.TargetUnreachable();

                return;
            }

            // activate loop particle once charged
            if (!_isCharged)
            {
                _isCharged = true;
                lightningLoopParticle.SetActive(true);
            }


            if (OutOfAngle) return;

            if (TargetInSight)
            {
                PlayShootAnimation();
                _nextShootTime = FireRate();

                for (var k = 0; k < BulletsNumber; k++)
                {
                    var bullet = _bulletPool
                        .Get(new PooledObjectSettings().SetPosition(shootPoint.position)
                            .SetRotation(CharacterBehaviour.transform.eulerAngles))
                        .GetComponent<TeslaBulletBehavior>();
                    bullet.Initialise(
                        damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier *
                        CharacterBehaviour.critMultiplier,
                        _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, 5f, false, stunDuration);
                    bullet.SetTargetsHitGoal(targetsHitGoal.Random());
                }


                CancelCharge();
            }
            else
            {
                TargetUnreachable();
            }
        }

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

        public override void OnGunUnloaded()
        {

        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.TeslaHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            _bulletPool.ReturnToPoolEverything();
        }
    }
}