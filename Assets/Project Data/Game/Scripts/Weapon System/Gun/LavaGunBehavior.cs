using UnityEngine;
using Watermelon.Upgrades;

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

        float attackDelay;
        DuoFloat bulletSpeed;

        float nextShootTime;

        Pool bulletPool;

        TweenCase shootTweenCase;

        float shootingRadius;
        LavaLauncherUpgrade upgrade;

        public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
        {
            base.Initialise(characterBehaviour, data);

            upgrade = UpgradesController.GetUpgrade<LavaLauncherUpgrade>(data.UpgradeType);
            var bulletObj = (upgrade.CurrentStage as BaseWeaponUpgradeStage).BulletPrefab;

            bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

            shootingRadius = characterBehaviour.EnemyDetector.DetectorRadius;

            RecalculateDamage();
        }

        public override void OnLevelLoaded()
        {
            RecalculateDamage();
        }

        public override void RecalculateDamage()
        {
            var stage = upgrade.GetCurrentStage();

            damage = stage.Damage;
            attackDelay = 1f / stage.FireRate;
            bulletSpeed = stage.BulletSpeed;
        }

        public override void GunUpdate()
        {
            if (!characterBehaviour.IsCloseEnemyFound)
                return;

            if (nextShootTime >= Time.timeSinceLevelLoad)
                return;

            var shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) - shootPoint.position;
            var origin = shootPoint.position - shootDirection.normalized * 1.5f;

            if (Physics.Raycast(origin, shootDirection, out var hitInfo, 300f, targetLayers) && hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (Vector3.Angle(shootDirection, transform.forward) < 40f)
                {
                    characterBehaviour.SetTargetActive();

                    shootTweenCase.KillActive();

                    shootTweenCase = graphicsTransform.DOLocalMoveZ(-0.15f, attackDelay * 0.1f).OnComplete(delegate
                    {
                        shootTweenCase = graphicsTransform.DOLocalMoveZ(0, attackDelay * 0.15f);
                    });

                    shootParticleSystem.Play();
                    nextShootTime = Time.timeSinceLevelLoad + attackDelay;

                    var bulletsNumber = upgrade.GetCurrentStage().BulletsPerShot.Random();

                    for (var i = 0; i < bulletsNumber; i++)
                    {
                        var bullet = bulletPool.GetPooledObject(new PooledObjectSettings().SetPosition(shootPoint.position).SetEulerRotation(shootPoint.eulerAngles)).GetComponent<LavaBulletBehavior>();
                        bullet.Initialise(damage, bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, -1f, false, shootingRadius, characterBehaviour, bulletHeight, explosionRadius);
                    }

                    characterBehaviour.OnGunShooted();
                    characterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

                    AudioController.PlaySound(AudioController.Sounds.shotLavagun, 0.8f);
                }
            }
            else
            {
                characterBehaviour.SetTargetUnreachable();
            }
        }

        public override void OnGunUnloaded()
        {
            // Destroy bullets pool
            if (bulletPool != null)
            {
                bulletPool.Clear();
                bulletPool = null;
            }
        }

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.RocketHolderTransform);
            transform.ResetLocal();
        }

        public override void Reload()
        {
            bulletPool.ReturnToPoolEverything();
        }
    }
}