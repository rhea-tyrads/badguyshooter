using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class CrossBowBehaviour : BaseGunBehavior
{
    [SerializeField] float bulletDisableTime;
    float _spread;
    CrossbowUpgrade _upgrade;
    TweenCase _shootTweenCase;

    public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
    {
        base.Initialise(characterBehaviour, data);
        _upgrade = UpgradesController.Get<CrossbowUpgrade>(data.UpgradeType);
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
        _attackDelay = 1f / stage.FireRate;
        _spread = stage.Spread;
        _bulletSpeed = stage.BulletSpeed;
    }
    void PlayShootAnimation()
    {
        _shootTweenCase.KillActive();
        _shootTweenCase = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f / CharacterBehaviour.AtkSpdMult).OnComplete(delegate { _shootTweenCase = transform.DOLocalMoveZ(0, _attackDelay * 0.6f / CharacterBehaviour.AtkSpdMult); });
        if (shootParticleSystem)  shootParticleSystem.Play();
        CharacterBehaviour.FocusOnTarget();
        CharacterBehaviour.OnGunShooted();
        AudioController.Play(AudioController.Sounds.shotMinigun);
    }
    
    int BulletsNumber => RandomBulletsAmount(_upgrade);
    
    public override void GunUpdate()
    {
        if (NoTarget) return;
        if (NotReady) return;

        _shootDirection = AimAtTarget();
        if (OutOfAngle) return;

        if (TargetInSight)
        {
            PlayShootAnimation();
            _nextShootTime = FireRate();
 
            for (var k = 0; k < BulletsNumber; k++)
            {
                foreach (var streamAngle in bulletStreamAngles)
                {
                    var bullet = _bulletPool.Get(
                            new PooledObjectSettings()
                                .SetPosition(shootPoint.position)
                                .SetRotation(CharacterBehaviour.transform.eulerAngles + Vector3.up *
                                    (Random.Range(-_spread, _spread) +
                                     streamAngle)))
                        .GetComponent<CrossBowBulletBehaviour>();
                    bullet.Initialise(
                        damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier *
                        CharacterBehaviour.critMultiplier,
                        _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, bulletDisableTime);
                    bullet.owner = Owner;
                }
            }
        }
        else
        {
            TargetUnreachable();
        }
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