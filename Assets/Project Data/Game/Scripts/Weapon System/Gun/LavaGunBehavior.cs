using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class LavaGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] Transform graphicsTransform;
        [SerializeField] ParticleSystem shootParticleSystem;

        [SerializeField] LayerMask targetLayers;
        [SerializeField] float explosionRadius;
        [SerializeField] DuoFloat bulletHeight;

        float _attackDelay;
        DuoFloat _bulletSpeed;

        float _nextShootTime;

        Pool _bulletPool;

        TweenCase _shootTweenCase;

        float _shootingRadius;
        LavaLauncherUpgrade _upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            _upgrade = UpgradesController.Get<LavaLauncherUpgrade>(data.UpgradeType);
            var bulletObj = _upgrade.BulletPrefab;

            _bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

            _shootingRadius = characterBehaviour.EnemyDetector.DetectorRadius;

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
            _bulletSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            if (!CharacterBehaviour.IsCloseEnemyFound) return;
            if (_nextShootTime >= Time.timeSinceLevelLoad) return;

            var shootDirection = CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
            var origin = shootPoint.position - shootDirection.normalized * 1.5f;

            if (Physics.Raycast(origin, shootDirection, out var hitInfo, 300f, targetLayers) && hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (Vector3.Angle(shootDirection, transform.forward) < 40f)
                {
                    CharacterBehaviour.SetTargetActive();

                    _shootTweenCase.KillActive();

                    _shootTweenCase = graphicsTransform.DOLocalMoveZ(-0.15f, _attackDelay * 0.1f).OnComplete(delegate
                    {
                        _shootTweenCase = graphicsTransform.DOLocalMoveZ(0, _attackDelay * 0.15f);
                    });

                    shootParticleSystem.Play();
                    _nextShootTime = Time.timeSinceLevelLoad + _attackDelay / CharacterBehaviour.AtkSpdMult;

                    var bulletsNumber = _upgrade.GetCurrentStage().BulletsPerShot.Random();

                    for (var i = 0; i < bulletsNumber; i++)
                    {
                        var bullet = _bulletPool.Get(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(shootPoint.eulerAngles)).GetComponent<LavaBulletBehavior>();
                        bullet.Initialise(damage, _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, -1f, false, _shootingRadius, CharacterBehaviour, bulletHeight, explosionRadius);
                    }

                    CharacterBehaviour.OnGunShooted();
                    CharacterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                    AudioController.Play(AudioController.Sounds.shotLavagun, 0.8f);
                }
            }
            else
            {
                CharacterBehaviour.SetTargetUnreachable();
            }
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
            transform.SetParent(characterGraphics.RocketHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            _bulletPool.ReturnToPoolEverything();
        }
    }
}