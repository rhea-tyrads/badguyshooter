using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Watermelon;
using Watermelon.SquadShooter;
using Random = UnityEngine.Random;

public class DuoPistolBehaviour : BaseGunBehavior
{
    [LineSpacer]
    [SerializeField] ParticleSystem shootParticleSystem2;
    [SerializeField] Transform shootPoint2;
    [SerializeField] float bulletDisableTime;
    [SerializeField] float reloadTime = 0.1f;
    float _offsetDir = 1;
    bool _isLeft;
    float _reloadTimer;
    const int BULLETS_TOTAL = 6;
    int _bullets;
    float _spread;
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

    void PlayShootAnimation()
    {
        _shootTweenCase.KillActive();
        _shootTweenCase = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f).OnComplete(delegate { _shootTweenCase = transform.DOLocalMoveZ(0, _attackDelay * 0.6f); });
        if (shootParticleSystem) shootParticleSystem.Play();
        CharacterBehaviour.FocusOnTarget();
        CharacterBehaviour.OnGunShooted();
        AudioController.Play(AudioController.Sounds.shotMinigun);
    }

    int BulletsNumber => RandomBulletsAmount(_upgrade);

    void Update()
    {
        if (NoTarget) return;
        if (NotReady) return;
        if (_bullets > 0) return;

        if (_reloadTimer > 0)
        {
            _reloadTimer -= Time.deltaTime;
            return;
        }

        _bullets = BULLETS_TOTAL;
    }

    public override void Shoot()
    {
        var shootPos = _isLeft ? shootPoint : shootPoint2;

        PlayShootAnimation();

        // if (isLeft) shootParticleSystem.Play();
        // else shootParticleSystem_2.Play();

        var finalSpread = CharacterBehaviour.isMultishotBooster && _spread == 0 ? 30 : _spread;

        for (var k = 0; k < BulletsNumber; k++)
        {
            foreach (var streamAngle in bulletStreamAngles)
            {
                var bullet = _bulletPool.Get(new PooledObjectSettings().SetPosition(shootPos.position).SetRotation(CharacterBehaviour.transform.eulerAngles + Vector3.up *
                    (Random.Range(-finalSpread, finalSpread) +
                     streamAngle))).GetComponent<DuoPistolsBulletBehaviour>();
                bullet.Initialise(damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier,
                    _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, bulletDisableTime, false);
                bullet.owner = Owner;
            }
        }

        _bullets--;
        if (_bullets <= 0) _reloadTimer = reloadTime;
        //isLeft = !isLeft;
    }


    public override void PlaceGun(BaseCharacterGraphics characterGraphics)
    {
        transform.SetParent(characterGraphics.MinigunHolderTransform);
        transform.ResetLocal();
    }

}