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
    Vector3 _shootDirection;

    [SerializeField] LayerMask targetLayers;
    [SerializeField] ParticleSystem shootParticleSystem;
    public float attackDelay = 3f;
    public float shootFrequency = 0.1f;
    public float damage;
    public float bulletSpeed;

    bool _isShooting;
    public int bulletsNumber;
    public Transform model;
    public GameObject bulletPrefab;

    void Start()
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
    float _reloadTimer;
    float _attackTimer;

    void Update()
    {
        if ( characterBehaviour.IsCloseEnemyFound)
        {
            var enemyPos = characterBehaviour.ClosestEnemyBehaviour.transform.position;
            _shootDirection = enemyPos.SetY(shootPoint.position.y) - shootPoint.position;
            model.forward = _shootDirection;
        
        }
 

        if (_isShooting) return;

        if (_isReloading)
        {
            _reloadTimer -= Time.deltaTime;
            if (_reloadTimer <= 0)
                ReloadFinish();
            return;
        }

        if (_attackTimer >= 0)
        {
            _attackTimer -= Time.deltaTime;
            return;
        }


        // if (Physics.Raycast(transform.position, shootDirection, out var hitInfo, 300f, targetLayers) &&
        //     hitInfo.collider.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
        // {

        //  return;
        // if (!(Vector3.Angle(shootDirection, transform.forward.SetY(0f)) < 40f)) return;

        //  shootParticleSystem.Play();
        if ( !characterBehaviour.IsCloseEnemyFound)return;
        _attackTimer = attackDelay / characterBehaviour.AtkSpdMult;
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

        AudioController.Play(AudioController.Sounds.shotMinigun);
        //  }
    }

    IEnumerator Shoot()
    {
        _isShooting = true;

        _bullets--;

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
            AudioController.Play(AudioController.Sounds.shotTesla);
            
            Debug.LogError("Shoot - " + i);
        }

        if (_bullets <= 0) Reload();
        _isShooting = false;
    }

    public int magazine = 6;
    int _bullets;
    bool _isReloading;

    void ReloadFinish()
    {
        _isReloading = false;
        _bullets = magazine;
    }

    void Reload()
    {
        _reloadTimer = reloadDuration;
        _isReloading = true;
        //  Invoke(nameof(ReloadFinish),attackTimer);
    }

    public List<FlyingDroneBulletBehaviour> lastBullet = new();
}