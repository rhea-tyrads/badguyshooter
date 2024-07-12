using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class FlyingDroneAbility : MonoBehaviour
{
    [SerializeField] Transform shootPoint;
    [SerializeField] CharacterBehaviour characterBehaviour;

    [SerializeField] Pool bulletPool;
    Vector3 shootDirection;

    [SerializeField] LayerMask targetLayers;
    [SerializeField] ParticleSystem shootParticleSystem;
    public float attackDelay = 3f;
    public float shootFrequency = 0.1f;
    public float damage;
    public float bulletSpeed;

    private bool IsShooting;
    public int bulletsNumber;
    public Transform model;
    public GameObject bulletPrefab;

    private void Start()
    {
        characterBehaviour = CharacterBehaviour.GetBehaviour();
        bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 5, true));
        ReloadFinish();
//        shootingRadius = characterBehaviour.EnemyDetector.DetectorRadius;
    }

    void UpdateRotation(Vector3 rotation)
    {
        model.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }

    public float reloadDuration = 5f;
    float reloadTimer;
    private float attackTimer;

    void Update()
    {
        if ( characterBehaviour.IsCloseEnemyFound)
        {
            var enemyPos = characterBehaviour.ClosestEnemyBehaviour.transform.position;
            shootDirection = enemyPos.SetY(shootPoint.position.y) - shootPoint.position;
            model.forward = shootDirection;
        
        }
 

        if (IsShooting) return;

        if (IsReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
                ReloadFinish();
            return;
        }

        if (attackTimer >= 0)
        {
            attackTimer -= Time.deltaTime;
            return;
        }


        // if (Physics.Raycast(transform.position, shootDirection, out var hitInfo, 300f, targetLayers) &&
        //     hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
        // {

        //  return;
        // if (!(Vector3.Angle(shootDirection, transform.forward.SetY(0f)) < 40f)) return;

        //  shootParticleSystem.Play();
        if ( !characterBehaviour.IsCloseEnemyFound)return;
        attackTimer = attackDelay / characterBehaviour.AtkSpdMult;
        StartCoroutine(Shoot());
        return;
        for (var k = 0; k < bulletsNumber; k++)
        {
            var bullet = bulletPool.Get(new PooledObjectSettings()
                    .SetPosition(shootPoint.position)
                    .SetEulerRotation(characterBehaviour.transform.eulerAngles + Vector3.up))
                .GetComponent<FlyingDroneBulletBehaviour>();
            lastBullet.Add(bullet);

            bullet.Initialise(
                damage * characterBehaviour.Stats.BulletDamageMultiplier * characterBehaviour.critMultiplier,
                bulletSpeed, characterBehaviour.ClosestEnemyBehaviour, 10f);
            bullet.owner = characterBehaviour;
        }

        AudioController.PlaySound(AudioController.Sounds.shotMinigun);
        //  }
    }

    IEnumerator Shoot()
    {
        IsShooting = true;

        bullets--;

        for (int i = 0; i < bulletsNumber; i++)
        {
            yield return new WaitForSeconds(shootFrequency);

            if (!characterBehaviour.IsCloseEnemyFound) continue;
            if (characterBehaviour.ClosestEnemyBehaviour.IsDead) continue;

            var bullet = bulletPool.Get(new PooledObjectSettings()
                    .SetPosition(shootPoint.position)
                    .SetEulerRotation(characterBehaviour.transform.eulerAngles + Vector3.up))
                .GetComponent<FlyingDroneBulletBehaviour>();
          //  lastBullet.Add(bullet);

            bullet.Initialise(
                damage * characterBehaviour.Stats.BulletDamageMultiplier * characterBehaviour.critMultiplier,
                bulletSpeed, characterBehaviour.ClosestEnemyBehaviour, 10f);
            bullet.owner = characterBehaviour;
            AudioController.PlaySound(AudioController.Sounds.shotTesla);
            
            Debug.LogError("Shoot - " + i);
        }

        if (bullets <= 0) Reload();
        IsShooting = false;
    }

    public int magazine = 6;
    private int bullets;
    private bool IsReloading;

    void ReloadFinish()
    {
        IsReloading = false;
        bullets = magazine;
    }

    void Reload()
    {
        reloadTimer = reloadDuration;
        IsReloading = true;
        //  Invoke(nameof(ReloadFinish),attackTimer);
    }

    public List<FlyingDroneBulletBehaviour> lastBullet = new();
}