using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Watermelon;
using Watermelon.SquadShooter;

public class RevolverBehaviour : BaseGunBehavior
{
    [LineSpacer]
    [SerializeField] ParticleSystem shootParticleSystem2;
    [SerializeField] Transform shootPoint2;
    [SerializeField] float bulletDisableTime;
    [Space]
    [SerializeField] Vector3 shootPointOffset;
    [SerializeField] float reloadTime = 0.1f;
    const int BULLETS_TOTAL = 6;
    float _offsetDir = 1;
    bool _isLeft;
    float _reloadTimer;
    int _bullets;
    float _spread;
    RevolverUpgrade _upgrade;
    TweenCase _shootTweenCase;

    public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
    {
        base.Initialise(characterBehaviour, data);
        _upgrade = UpgradesController.Get<RevolverUpgrade>(data.UpgradeType);
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

    int BulletsNumber => RandomBulletsAmount(_upgrade);

    public override void GunUpdate()
    {
        var shootPos = _isLeft ? shootPoint : shootPoint2;
        if (NoTarget) return;
        if (NotReady) return;

        if (_bullets <= 0)
        {
            if (_reloadTimer > 0)
            {
                _reloadTimer -= Time.fixedDeltaTime;
                return;
            }

            _bullets = BULLETS_TOTAL;
        }

        // offsetDir *= -1;
        // shootPoint.localPosition += offsetDir * shootPointOffset;
        _shootDirection = AimAtTarget();
        if (OutOfAngle) return;

        if (TargetInSight)
        {
            //   if (isLeft) shootParticleSystem.Play();
            // else shootParticleSystem_2.Play();

            PlayShootAnimation();
            _nextShootTime = FireRate();

            var finalSpread = CharacterBehaviour.isMultishotBooster && _spread == 0 ? 30 : _spread;

            for (var k = 0; k < BulletsNumber; k++)
            {
                foreach (var streamAngle in bulletStreamAngles)
                {
                    var bullet = _bulletPool.Get(
                            new PooledObjectSettings()
                                .SetPosition(shootPos.position)
                                .SetRotation(CharacterBehaviour.transform.eulerAngles + Vector3.up *
                                    (Random.Range(-finalSpread, finalSpread) +
                                     streamAngle)))
                        .GetComponent<RevolverBulletBehaviour>();
                    bullet.Initialise(damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier,
                        _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                    bullet.owner = Owner;
                }
            }

            _bullets--;
            if (_bullets <= 0) _reloadTimer = reloadTime;
            // isLeft = !isLeft;
        }
        else
        {
            TargetUnreachable();
        }
    }

    void PlayShootAnimation()
    {
        _shootTweenCase.KillActive();
        _shootTweenCase = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f).OnComplete(delegate { _shootTweenCase = transform.DOLocalMoveZ(0, _attackDelay * 0.6f); });
        if (shootParticleSystem) shootParticleSystem.Play();
        CharacterBehaviour.FocusOnTarget();
        CharacterBehaviour.OnGunShooted();
        AudioController.Play(AudioController.Sounds.shotMinigun);
    }

    public override void OnGunUnloaded()
    {
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