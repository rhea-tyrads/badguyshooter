using System.Collections.Generic;
using TopDownEngine.Common.Scripts.BoogieScripts;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class LaserGunBehaviour : BaseGunBehavior
{
    [LineSpacer]
    [SerializeField] ParticleSystem shootParticleSystem;
    [SerializeField] LayerMask targetLayers;
    [SerializeField] LayerMask obstacleLayers;
    [SerializeField] float bulletDisableTime;
    [SerializeField] MagicBeam laser;

    [Space]
    [SerializeField] List<float> bulletStreamAngles;

    float _spread;
    float _attackDelay;
    DuoFloat _bulletSpeed;
    float _nextShootTime;
    Pool _bulletPool;
    Vector3 _shootDirection;
    LaserUpgrade _upgrade;
    TweenCase _shootTweenCase;

    public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
    {
        base.Initialise(characterBehaviour, data);
        _upgrade = UpgradesController.Get<LaserUpgrade>(data.UpgradeType);
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
        var atkSpdMult = CharacterBehaviour.isAtkSpdBooster ? CharacterBehaviour.atkSpdBoosterMult : 1;
        _attackDelay = 1f / (stage.FireRate * atkSpdMult);
        _spread = stage.Spread;
        _bulletSpeed = stage.BulletSpeed;
    }

    public override void GunUpdate()
    {
        if (!CharacterBehaviour.IsCloseEnemyFound)
        {
            laser.Hide();
            return;
        }

        if (_nextShootTime >= Time.timeSinceLevelLoad) return;
        _shootDirection = CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) -
                         shootPoint.position;


        if (Physics.Raycast(transform.position, _shootDirection, out var hitInfo, 300f, targetLayers) &&
            hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
        {
            if (!(Vector3.Angle(_shootDirection, transform.forward.SetY(0f)) < 40f)) return;

            laser.Show(CharacterBehaviour.ClosestEnemyBehaviour.transform);
            _shootTweenCase.KillActive();

            _shootTweenCase = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f).OnComplete(delegate
            {
                _shootTweenCase = transform.DOLocalMoveZ(0, _attackDelay * 0.6f);
            });

            CharacterBehaviour.SetTargetActive();
            shootParticleSystem.Play();
            _nextShootTime = Time.timeSinceLevelLoad + _attackDelay / CharacterBehaviour.AtkSpdMult;

            if (bulletStreamAngles.IsNullOrEmpty())
                bulletStreamAngles = new List<float> {0};

            var bulletsNumber = _upgrade.GetCurrentStage().BulletsPerShot.Random();
            var dmgBonus = (_upgrade.GetCurrentStage().BulletsPerShot.Random() +
                            CharacterBehaviour.MultishotBoosterAmount) > 1
                ? 3
                : 1f;
            for (var k = 0; k < bulletsNumber; k++)
            {
                foreach (var streamAngle in bulletStreamAngles)
                {
                    var bullet = _bulletPool.Get(new PooledObjectSettings()
                            .SetPosition(shootPoint.position)
                            .SetEulerRotation(CharacterBehaviour.transform.eulerAngles + Vector3.up *
                                (Random.Range(-_spread, _spread) + streamAngle)))
                        .GetComponent<LaserBulletBehaviour>();

                    bullet.Initialise(
                        damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier *
                        CharacterBehaviour.critMultiplier * dmgBonus,
                        _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                    bullet.owner = Owner;
                }
            }

            CharacterBehaviour.OnGunShooted();
            AudioController.Play(AudioController.Sounds.shotMinigun);

            return;
            var laserDir = (CharacterBehaviour.ClosestEnemyBehaviour.transform.position - transform.position)
                .normalized;
            if (Physics.Raycast(transform.position, laserDir, out var obstacleInfo, 9000f, obstacleLayers))
            {
                laser.Show(obstacleInfo.transform);
            }
        }
        else
        {
            CharacterBehaviour.SetTargetUnreachable();
        }
    }

    public override void OnGunUnloaded()
    {
        if (_bulletPool == null) return;
        _bulletPool.Clear();
        _bulletPool = null;
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