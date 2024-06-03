using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class FlamethrowerBehaviour : BaseGunBehavior
{
    [LineSpacer]
    
    //[SerializeField] ParticleSystem shootParticleSystem;
    [SerializeField] LayerMask targetLayers;
    [SerializeField] float bulletDisableTime;

 
    [Space]
    [SerializeField] List<float> bulletStreamAngles;

    float spread;
    float attackDelay;
    DuoFloat bulletSpeed;
    float nextShootTime;
    Pool bulletPool;
    Vector3 shootDirection;
    FlameThrowerUpgrade upgrade;
    TweenCase shootTweenCase;

    public override void Initialise(CharacterBehaviour characterBehaviour, WeaponData data)
    {
        base.Initialise(characterBehaviour, data);

        upgrade = UpgradesController.GetUpgrade<FlameThrowerUpgrade>(data.UpgradeType);

        var bulletObj = upgrade.BulletPrefab;
        bulletPool = new Pool(new PoolSettings(bulletObj.name, bulletObj, 5, true));

        RecalculateDamage();
    }

    public override void OnLevelLoaded()
    {
        RecalculateDamage();
    }

    public override void RecalculateDamage()
    {
        var stage = upgrade.GetCurrentStage();

        damage = stage.Damage;
        attackDelay = 1f / stage.FireRate;
        spread = stage.Spread;
        bulletSpeed = stage.BulletSpeed;
    }

    public GameObject flameParticle;
    
    public override void GunUpdate()
    {
        if (!characterBehaviour.IsCloseEnemyFound)
        {
            flameParticle.SetActive(false);
            return;
        }
        //  barrelTransform.Rotate(Vector3.forward * fireRotationSpeed);
        
        flameParticle.SetActive(true);
        if (nextShootTime >= Time.timeSinceLevelLoad) return;

        shootDirection = characterBehaviour.ClosestEnemyBehaviour.transform.position.SetY(shootPoint.position.y) -
                         shootPoint.position;

        if (Physics.Raycast(transform.position, shootDirection, out var hitInfo, 300f, targetLayers) &&
            hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
        {
            if (Vector3.Angle(shootDirection, transform.forward.SetY(0f)) < 40f)
            {
                shootTweenCase.KillActive();

                shootTweenCase = transform.DOLocalMoveZ(-0.0825f, attackDelay * 0.3f).OnComplete(delegate
                {
                    shootTweenCase = transform.DOLocalMoveZ(0, attackDelay * 0.6f);
                });

                characterBehaviour.SetTargetActive();

             //   shootParticleSystem.Play();

                nextShootTime = Time.timeSinceLevelLoad + attackDelay / characterBehaviour.AtkSpdMult;

                if (bulletStreamAngles.IsNullOrEmpty())
                {
                    bulletStreamAngles = new List<float> {0};
                }

                var bulletsNumber = upgrade.GetCurrentStage().BulletsPerShot.Random();

                for (var k = 0; k < bulletsNumber; k++)
                {
                    foreach (var streamAngle in bulletStreamAngles)
                    {
                        var bullet = bulletPool.Get(
                                new PooledObjectSettings()
                                    .SetPosition(shootPoint.position)
                                    .SetEulerRotation(characterBehaviour.transform.eulerAngles + Vector3.up *
                                        (Random.Range(-spread, spread) +
                                         streamAngle)))
                            .GetComponent<FlamethrowerBulletBehaviour>();

                        bullet.Initialise(damage.Random() * characterBehaviour.Stats.BulletDamageMultiplier* characterBehaviour.critMultiplier,
                            bulletSpeed.Random(), characterBehaviour.ClosestEnemyBehaviour, bulletDisableTime, false);
                        bullet.owner = Owner;
                    }
                }


                characterBehaviour.OnGunShooted();

                AudioController.PlaySound(AudioController.Sounds.shotMinigun);
            }
        }
        else
        {
            characterBehaviour.SetTargetUnreachable();
        }
    }

    public override void OnGunUnloaded()
    {
        // Destroy bullets pool
        if (bulletPool != null)
        {
            bulletPool.Clear();
            bulletPool = null;
        }
    }

    public override void PlaceGun(BaseCharacterGraphics characterGraphics)
    {
        transform.SetParent(characterGraphics.MinigunHolderTransform);
        transform.ResetLocal();
    }

    public override void Reload()
    {
        bulletPool.ReturnToPoolEverything();
    }
}