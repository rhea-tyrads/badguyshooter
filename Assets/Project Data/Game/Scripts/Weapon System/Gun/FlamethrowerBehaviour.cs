using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Watermelon;
using Watermelon.SquadShooter;
using Random = UnityEngine.Random;

public class FlamethrowerBehaviour : BaseGunBehavior
{
    [LineSpacer]
    [SerializeField] float bulletDisableTime;
    [Space]
    [SerializeField] GameObject flameParticle;
    [SerializeField] GameObject flameParticleMultishot3;
    [SerializeField] GameObject flameParticleMultishot5;
 
    FlameThrowerUpgrade _upgrade;
 

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
        if (shootParticleSystem) shootParticleSystem.Play();
        CharacterBehaviour.FocusOnTarget();
        CharacterBehaviour.OnGunShooted();
        AudioController.Play(AudioController.Sounds.shotMinigun);
    }

    void Update()
    {
        if (NoTarget)
        {
            flameParticle.SetActive(false);
            flameParticleMultishot3.SetActive(false);
            flameParticleMultishot5.SetActive(false);
            return;
        }

        flameParticle.SetActive(true);
    }

    public override void Shoot()
    {
        PlayShootAnimation();

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
        
        for (var i = 0; i < BulletsNumber; i++)
        {
            SpawnBullet(i);
        }
    }

    int BulletsNumber => RandomBulletsAmount(_upgrade);


    public override void PlaceGun(BaseCharacterGraphics characterGraphics)
    {
        transform.SetParent(characterGraphics.MinigunHolderTransform);
        transform.ResetLocal();
    }

}