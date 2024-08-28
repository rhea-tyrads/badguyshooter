using System;
using System.Collections.Generic;
using TopDownEngine.Common.Scripts.BoogieScripts;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;
using Random = UnityEngine.Random;

public class LaserGunBehaviour : BaseGunBehavior
{
    [LineSpacer]
    [SerializeField] LayerMask obstacleLayers;
    [SerializeField] float bulletDisableTime;
    [SerializeField] MagicBeam laser;
    [Space]
 
    LaserUpgrade _upgrade;
 

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
        if (NoTarget)
        {
            laser.Hide();
            return;
        }
    }

    public override void Shoot()
    {
        PlayShootAnimation();
        laser.Show(CharacterBehaviour.ClosestEnemyBehaviour.transform);
 
        
        for (var i = 0; i < BulletsNumber; i++)
        {
            SpawnBullet(i);
        }
        
 
    }


    public override void PlaceGun(BaseCharacterGraphics characterGraphics)
    {
        transform.SetParent(characterGraphics.MinigunHolderTransform);
        transform.ResetLocal();
    }

}