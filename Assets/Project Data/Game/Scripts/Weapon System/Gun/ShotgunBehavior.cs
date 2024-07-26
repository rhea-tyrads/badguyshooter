using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShotgunBehavior : BaseGunBehavior
    {
        [LineSpacer]
        [SerializeField] float bulletDisableTime;
        float _bulletSpreadAngle;
        TweenCase _shootTweenCase;
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

        int BulletsNumber => RandomBulletsAmount(_upgrade);

        void PlayShootAnimation()
        {
            _shootTweenCase.KillActive();
            _shootTweenCase = transform.DOLocalMoveZ(-0.15f, 0.1f).OnComplete(delegate { _shootTweenCase = transform.DOLocalMoveZ(0, 0.15f); });
            if (shootParticleSystem) shootParticleSystem.Play();
            CharacterBehaviour.FocusOnTarget();
            CharacterBehaviour.OnGunShooted();
            AudioController.Play(AudioController.Sounds.shotMinigun);

            CharacterBehaviour.MainCameraCase.Shake(0.04f, 0.04f, 0.3f, 0.8f);
        }

        public override void Shoot()
        {
            PlayShootAnimation();

            for (var i = 0; i < BulletsNumber; i++)
            {
                var settings = PoolSettings();
                var bullet = _bulletPool.GetPlayerBullet(settings);
                bullet.Initialise(Damage, BulletSpeed, Target, bulletLifeTime);
                bullet.transform.Rotate(new Vector3(0f, i == 0 ? 0f : Random.Range(_bulletSpreadAngle * -0.5f, _bulletSpreadAngle * 0.5f), 0f));
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

        public override void PlaceGun(BaseCharacterGraphics characterGraphics)
        {
            transform.SetParent(characterGraphics.ShootGunHolderTransform);
            transform.ResetLocal();
        }
    }
}