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

    public override void Shoot()
    {
        var shootPos = _isLeft ? shootPoint : shootPoint2;

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
        //   if (isLeft) shootParticleSystem.Play();
        // else shootParticleSystem_2.Play();

        PlayShootAnimation();

        var finalSpread = CharacterBehaviour.isMultishotBooster && _spread == 0 ? 30 : _spread;

        for (var k = 0; k < BulletsNumber; k++)
        {
            foreach (var streamAngle in bulletStreamAngles)
            {
                var angle = Vector3.up * (Random.Range(-finalSpread, finalSpread) + streamAngle);
                var settings = PoolSettings(angle);
                var bullet = _bulletPool.GetPlayerBullet(settings);
                bullet.Initialise(Damage, BulletSpeed, Target, bulletLifeTime);
            }
        }

        _bullets--;
        if (_bullets <= 0) _reloadTimer = reloadTime;
        // isLeft = !isLeft;
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

    public override void PlaceGun(BaseCharacterGraphics characterGraphics)
    {
        transform.SetParent(characterGraphics.MinigunHolderTransform);
        transform.ResetLocal();
    }
}