using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Watermelon;
using Watermelon.SquadShooter;

public class FlamethrowerBehaviour : BaseGunBehavior
{
    [LineSpacer]
    [SerializeField] float bulletDisableTime;
    [Space]
    [SerializeField] GameObject flameParticle;
    [SerializeField] GameObject flameParticleMultishot3;
    [SerializeField] GameObject flameParticleMultishot5;
    
    float _spread;
    FlameThrowerUpgrade _upgrade;
    TweenCase _shootTweenCase;

    public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
    {
        base.Initialise(characterBehaviour, data);
        _upgrade = UpgradesController.Get<FlameThrowerUpgrade>(data.UpgradeType);
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
        _shootTweenCase = transform.DOLocalMoveZ(-0.0825f, _attackDelay * 0.3f).OnComplete(delegate { _shootTweenCase = transform.DOLocalMoveZ(0, _attackDelay * 0.6f); });
        if (shootParticleSystem)  shootParticleSystem.Play();
        CharacterBehaviour.FocusOnTarget();
        CharacterBehaviour.OnGunShooted();
        AudioController.Play(AudioController.Sounds.shotMinigun);
    }
    public override void GunUpdate()
    {
        if (NoTarget)
        {
            flameParticle.SetActive(false);
            flameParticleMultishot3.SetActive(false);
            flameParticleMultishot5.SetActive(false);
            return;
        }
        //  barrelTransform.Rotate(Vector3.forward * fireRotationSpeed);

        flameParticle.SetActive(true);
        if (NotReady) return;
        
        _shootDirection = AimAtTarget();
        if (OutOfAngle) return;

        if (TargetInSight)
        {
            PlayShootAnimation();
            _nextShootTime = FireRate();
            
    
            if (bulletStreamAngles.IsNullOrEmpty())
            {
                bulletStreamAngles = new List<float> { 0 };
            }

            var bulletsNumber = BulletsNumber;

            if (bulletsNumber == 1)
            {
                flameParticleMultishot3.SetActive(false);
                flameParticleMultishot5.SetActive(false);
            }

            if (bulletsNumber > 1)
                flameParticleMultishot3.SetActive(true);

            if (bulletsNumber > 3)
                flameParticleMultishot5.SetActive(true);

            var finalSpread = CharacterBehaviour.isMultishotBooster && _spread == 0 ? 30 : _spread;

            for (var k = 0; k < bulletsNumber; k++)
            {
                foreach (var streamAngle in bulletStreamAngles)
                {
                    var bullet = _bulletPool.Get(
                        new PooledObjectSettings()
                            .SetPosition(shootPoint.position)
                            .SetRotation(CharacterBehaviour.transform.eulerAngles + Vector3.up *
                                (Random.Range(-finalSpread, finalSpread) +
                                 streamAngle))).GetComponent<FlamethrowerBulletBehaviour>();

                    bullet.Initialise(damage.Random() * CharacterBehaviour.Stats.BulletDamageMultiplier * CharacterBehaviour.critMultiplier,
                        _bulletSpeed.Random(), CharacterBehaviour.ClosestEnemyBehaviour, bulletDisableTime, false);
                }
            }
 
        }
        else
        {
           TargetUnreachable();
        }
    }
    int BulletsNumber => RandomBulletsAmount(_upgrade);
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