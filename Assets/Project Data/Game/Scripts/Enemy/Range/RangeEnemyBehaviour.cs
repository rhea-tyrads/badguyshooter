using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class RangeEnemyBehaviour : BaseEnemyBehavior
    {
        readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");

        [Header("Fighting")]
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] float bulletSpeed;
        [SerializeField] float hitCooldown;
        [SerializeField] LayerMask targetLayer;

        [Header("Weapon")]
        [SerializeField] Transform shootPointTransform;

        [Space]
        [SerializeField] ParticleSystem gunFireParticle;

        [Space]
        [SerializeField] bool canReload;
        public bool CanReload => canReload;

        bool isHitting;

        Pool bulletPool;

        protected override void Awake()
        {
            base.Awake();

            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));
        }

        public override void Attack()
        {
            animatorRef.SetTrigger("Shoot");
        }

        public override void Initialise()
        {
            base.Initialise();

            isDead = false;
            isHitting = false;
        }

        void PerformHit()
        {
            if (isHitting)
                return;

            isHitting = true;

            navMeshAgent.isStopped = true;

            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);

            Tween.DelayedCall(0.2f, () =>
            {
                if (!IsDead)
                    AudioController.Play(AudioController.Sounds.enemyShot, 0.6f);
            });
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!LevelController.IsGameplayActive)
                return;

            healthbarBehaviour.FollowUpdate();
        }

        public override void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead)
                return;

            base.TakeDamage(damage, projectilePosition, projectileDirection);
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType == EnemyCallbackType.Hit)
            {
                var bullet = bulletPool.Get(new PooledObjectSettings(false).SetPosition(shootPointTransform.position).SetEulerRotation(shootPointTransform.eulerAngles)).GetComponent<EnemyBulletBehavior>();
                bullet.transform.forward = transform.forward.SetY(0).normalized;
                bullet.Initialise(Damage, bulletSpeed, 200);

                gunFireParticle.Play();

                AudioController.Play(AudioController.Sounds.enemyShot);
            }
            else if (enemyCallbackType == EnemyCallbackType.HitFinish)
            {
                isHitting = false;
                InvokeOnAttackFinished();
            }
            else if (enemyCallbackType == EnemyCallbackType.ReloadFinished)
            {
                InvokeOnReloadFinished();
            }
        }
    }
}