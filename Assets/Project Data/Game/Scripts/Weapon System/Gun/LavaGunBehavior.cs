using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class LavaGunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] Transform graphicsTransform;
        [SerializeField] float explosionRadius;
        [SerializeField] DuoFloat bulletHeight;
 
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

        void PlayShootAnimation()
        {
            _shootTweenCase.KillActive();

            _shootTweenCase = graphicsTransform.DOLocalMoveZ(-0.15f, _attackDelay * 0.1f).OnComplete(delegate { _shootTweenCase = graphicsTransform.DOLocalMoveZ(0, _attackDelay * 0.15f); });

            if (shootParticleSystem) shootParticleSystem.Play();
            CharacterBehaviour.FocusOnTarget();
            CharacterBehaviour.OnGunShooted();
            CharacterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);

            AudioController.Play(AudioController.Sounds.shotLavagun, 0.8f);
        }

        int BulletsNumber => RandomBulletsAmount(_upgrade);

        public override void Shoot()
        {
            PlayShootAnimation();

            for (var i = 0; i < BulletsNumber; i++)
            {
                var bullet = _bulletPool.Get(new PooledObjectSettings().SetPosition(shootPoint.position).SetRotation(shootPoint.eulerAngles)).GetComponent<LavaBulletBehavior>();
                bullet.Initialise(damage, _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, -1f, false, _shootingRadius, CharacterBehaviour, bulletHeight, explosionRadius);
            }

        }


        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.RocketHolderTransform);
            transform.ResetLocal();
        }

    }
}