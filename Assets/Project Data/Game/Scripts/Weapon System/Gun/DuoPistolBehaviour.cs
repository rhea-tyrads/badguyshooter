using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Watermelon;
using Watermelon.SquadShooter;

public class DuoPistolBehaviour : BaseGunBehavior
{
    [LineSpacer]
    // [SerializeField] Transform barrelTransform;
    [SerializeField]
    ParticleSystem shootParticleSystem;

    [FormerlySerializedAs("shootParticleSystem_2")] [SerializeField] ParticleSystem shootParticleSystem2;
    [FormerlySerializedAs("shootPoint_2")] [SerializeField] Transform shootPoint2;

    [SerializeField] LayerMask targetLayers;
    [SerializeField] float bulletDisableTime;

    // [Space]
    // [SerializeField] float fireRotationSpeed;

    [Space] [SerializeField] List<float> bulletStreamAngles;
    [FormerlySerializedAs("RELOAD_TIME")] public float reloadTime = 0.1f;
    float _reloadTimer;
    const int BULLETS_TOTAL = 6;
    int _bullets;

    float _spread;
    float _attackDelay;
    DuoFloat _bulletSpeed;

    float _nextShootTime;

    Pool _bulletPool;

    Vector3 _shootDirection;

    DuoPistolUpgrade _upgrade;

    TweenCase _shootTweenCase;

    public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
    {
        base.Initialise(characterBehaviour, data);

        _upgrade = UpgradesController.Get<DuoPistolUpgrade>(data.UpgradeType);

        var bulletObj = _upgrade.BulletPrefab;
        _bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

        RecalculateDamage();

        _bullets = BULLETS_TOTAL;
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

    [FormerlySerializedAs("shootPointsOffset")]
    public Vector3 shootPointOffset;

    float _offsetDir = 1;
    bool _isLeft;

    public override void GunUpdate()
    {
        var shootPos = _isLeft ? shootPoint : shootPoint2;
        if (!CharacterBehaviour.IsCloseEnemyFound) return;
        //  barrelTransform.Rotate(Vector3.forward * fireRotationSpeed);
        if (_nextShootTime >= Time.timeSinceLevelLoad) return;

        if (_bullets <= 0)
        {
            if (_reloadTimer > 0)
            {
                _reloadTimer -= Time.fixedDeltaTime;
                return;
            }
            else
            {
                _bullets = BULLETS_TOTAL;
            }
        }

        // offsetDir *= -1;
        // shootPoint.localPosition += offsetDir * shootPointOffset;
        _shootDirection = CharacterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPos.position.y) -
                         shootPos.position;

        if (Physics.Raycast(transform.position, _shootDirection, out var hitInfo, 300f, targetLayers) &&
            hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
        {
            if (!(Vector3.Angle(_shootDirection, transform.forward.SetY(0f)) < 90f)) return;

            _shootTweenCase.KillActive();
            _shootTweenCase = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f).OnComplete(delegate
            {
                _shootTweenCase = transform.DOLocalMoveZ(0, _attackDelay * 0.6f);
            });

            CharacterBehaviour.SetTargetActive();
            shootParticleSystem.Play();
            // if (isLeft) shootParticleSystem.Play();
            // else shootParticleSystem_2.Play();

            _nextShootTime = Time.timeSinceLevelLoad + _attackDelay / CharacterBehaviour.AtkSpdMult;

            if (bulletStreamAngles.IsNullOrEmpty())
                bulletStreamAngles = new List<float> { 0 };

            var bulletsNumber = _upgrade.GetCurrentStage().BulletsPerShot.Random() +
                                CharacterBehaviour.MultishotBoosterAmount;
            var finalSpread = CharacterBehaviour.isMultishotBooster && _spread == 0 ? 30 : _spread;

            for (var k = 0; k < bulletsNumber; k++)
            {
                foreach (var streamAngle in bulletStreamAngles)
                {
                    var bullet = _bulletPool.Get(
                            new PooledObjectSettings()
                                .SetPosition(shootPos.position)
                                .SetEulerRotation(CharacterBehaviour.transform.eulerAngles + Vector3.up *
                                    (Random.Range(-finalSpread, finalSpread) +
                                     streamAngle)))
                        .GetComponent<DuoPistolsBulletBehaviour>();
                    bullet.Initialise(damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier,
                        _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, bulletDisableTime, false);
                    bullet.owner = Owner;
                }
            }

            _bullets--;
            if (_bullets <= 0) _reloadTimer = reloadTime;

            CharacterBehaviour.OnGunShooted();

            AudioController.Play(AudioController.Sounds.shotMinigun);
            //isLeft = !isLeft;
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
        transform.SetParent(characterGraphics.MinigunHolderTransform);
        transform.ResetLocal();
    }

    public override void Reload()
    {
        _bulletPool.ReturnToPoolEverything();
    }
}