using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class TeslaGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] ParticleSystem shootParticleSystem;
        [SerializeField] GameObject lightningLoopParticle;

        [SerializeField] LayerMask targetLayers;
        [SerializeField] float chargeDuration;
        DuoFloat _bulletSpeed;
        [SerializeField] DuoInt targetsHitGoal;
        [SerializeField] float stunDuration = 0.2f;

        Pool _bulletPool;

        TweenCase _shootTweenCase;
        Vector3 _shootDirection;

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
            // if no enemy - cancel charge
            if (!CharacterBehaviour.IsCloseEnemyFound)
            {
                if (_isCharging || _isCharged)
                {
                    CancelCharge();
                }

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

            // wait for full charge
            if (_fullChargeTime >= Time.timeSinceLevelLoad)
            {
                // start charge particle 0.5 sec before charge complete
                if (!_isChargeParticleActivated && _fullChargeTime - Time.timeSinceLevelLoad <= 0.5f)
                {
                    _isChargeParticleActivated = true;
                    shootParticleSystem.Play();
                }

                if (IsEnemyVisible()) CharacterBehaviour.SetTargetActive();
                else CharacterBehaviour.SetTargetUnreachable();

                return;
            }

            // activate loop particle once charged
            if (!_isCharged)
            {
                _isCharged = true;
                lightningLoopParticle.SetActive(true);
            }

            if (IsEnemyVisible())
            {
                CharacterBehaviour.SetTargetActive();

                _shootTweenCase.KillActive();

                _shootTweenCase = transform.DOLocalMoveZ(-0.15f, chargeDuration/ atkSpdMult * 0.3f).OnComplete(delegate
                {
                    _shootTweenCase = transform.DOLocalMoveZ(0, chargeDuration / atkSpdMult* 0.6f);
                });

                var bulletsNumber = _upgrade.GetCurrentStage().BulletsPerShot.Random();

                for (var k = 0; k < bulletsNumber; k++)
                {
                    var bullet = _bulletPool
                        .Get(new PooledObjectSettings().SetPosition(shootPoint.position)
                            .SetEulerRotation(CharacterBehaviour.transform.eulerAngles))
                        .GetComponent<TeslaBulletBehavior>();
                    bullet.Initialise(damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier* CharacterBehaviour.critMultiplier,
                        _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, 5f, false, stunDuration);
                    bullet.SetTargetsHitGoal(targetsHitGoal.Random());
                }

                CharacterBehaviour.OnGunShooted();
                CharacterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                CancelCharge();

                AudioController.Play(AudioController.Sounds.shotTesla, volumePercentage: 0.8f);
            }
            else
            {
                CharacterBehaviour.SetTargetUnreachable();
            }
        }

        public bool IsEnemyVisible()
        {
            if (!CharacterBehaviour.IsCloseEnemyFound)
                return false;

            _shootDirection = CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) -
                             shootPoint.position;

            RaycastHit hitInfo;
            if (Physics.Raycast(shootPoint.position - _shootDirection.normalized * 1.5f, _shootDirection, out hitInfo,
                    300f, targetLayers) ||
                Physics.Raycast(shootPoint.position, _shootDirection, out hitInfo, 300f, targetLayers)
               )
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    if (Vector3.Angle(_shootDirection, transform.forward) < 40f)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
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
            // Destroy bullets pool
            if (_bulletPool != null)
            {
                _bulletPool.Clear();
                _bulletPool = null;
            }
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