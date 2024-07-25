using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShotgunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] ParticleSystem shootParticleSystem;

        [SerializeField] LayerMask targetLayers;
        [SerializeField] float bulletDisableTime;

        float _attackDelay;
        DuoFloat _bulletSpeed;
        float _bulletSpreadAngle;

        float _nextShootTime;

        Pool _bulletPool;

        TweenCase _shootTweenCase;
        Vector3 _shootDirection;

        ShotgunUpgrade _upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            _upgrade = UpgradesController.Get<ShotgunUpgrade>(data.UpgradeType);

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
            _bulletSpreadAngle = stage.Spread;
            var atkSpdMult = CharacterBehaviour.isAtkSpdBooster ? CharacterBehaviour.atkSpdBoosterMult : 1;
            _attackDelay = 1f / (stage.FireRate * atkSpdMult);
            _bulletSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            // Combat
            if (!CharacterBehaviour.IsCloseEnemyFound) return;
            if (_nextShootTime >= Time.timeSinceLevelLoad) return;

            _shootDirection = CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            if (Physics.Raycast(transform.position, _shootDirection, out var hitInfo, 300f, targetLayers))
            {
                if (hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
                {
                    if (Vector3.Angle(_shootDirection, transform.forward) < 40f)
                    {
                        CharacterBehaviour.SetTargetActive();

                        _shootTweenCase.KillActive();

                        _shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.1f).OnComplete(delegate
                        {
                            _shootTweenCase = transform.DOLocalMoveZ(0, 0.15f);
                        });

                        shootParticleSystem.Play();

                        _nextShootTime = Time.timeSinceLevelLoad + _attackDelay / CharacterBehaviour.AtkSpdMult;

                        var bulletsNumber = _upgrade.GetCurrentStage().BulletsPerShot.Random();

                        for (var i = 0; i < bulletsNumber; i++)
                        {
                            var bullet = _bulletPool.Get(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(CharacterBehaviour.transform.eulerAngles)).GetComponent<PlayerBulletBehavior>();
                            bullet.Initialise(damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier* CharacterBehaviour.critMultiplier, _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                            bullet.transform.Rotate(new Vector3(0f, i == 0 ? 0f : Random.Range(_bulletSpreadAngle * -0.5f, _bulletSpreadAngle * 0.5f), 0f));
                        }

                        CharacterBehaviour.OnGunShooted();
                        CharacterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                        AudioController.Play(AudioController.Sounds.shotShotgun);
                    }
                }
                else
                {
                    CharacterBehaviour.SetTargetUnreachable();
                }
            }
            else
            {
                CharacterBehaviour.SetTargetUnreachable();
            }
        }

        void OnDrawGizmos()
        {
            if (CharacterBehaviour == null)
                return;

            if (CharacterBehaviour.ClosestEnemyBehaviour == null)
                return;

            var defCol = Gizmos.color;
            Gizmos.color = Color.red;

            var shootDirection = CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;

            Gizmos.DrawLine(shootPoint.position - shootDirection.normalized * 10f, CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y));

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
            transform.SetParent(characterGraphics.ShootGunHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            _bulletPool?.ReturnToPoolEverything();
        }
    }
}