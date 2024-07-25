using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class BossBomberBehaviour : BaseEnemyBehavior
    {
        protected static readonly int PARTICLE_BOSS_EAT_HASH = ParticlesController.GetHash("Boss Eat");

        readonly int PARTICLE_STEP_HASH = ParticlesController.GetHash("Boss Step");
        readonly int PARTICLE_DEATH_FALL_HASH = ParticlesController.GetHash("Boss Death Fall");
        readonly int PARTICLE_ENTER_FALL_HASH = ParticlesController.GetHash("Boss Enter Fall");
        readonly int PARTICLE_KICK_HASH = ParticlesController.GetHash("Boss Kick");

        readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");
        readonly int ANIMATOR_DIE_HASH = Animator.StringToHash("Death");
        readonly int ANIMATOR_ENTER_HASH = Animator.StringToHash("Enter");
        readonly int ANIMATOR_SHOOTING_HASH = Animator.StringToHash("Shooting");
        readonly int ANIMATOR_KICK_HASH = Animator.StringToHash("Kick");

        [SerializeField] GameObject graphicsObject;

        [Header("Fighting")]
        [SerializeField] GameObject bulletPrefab;
        [SerializeField] float bulletSpeed;
        [SerializeField] int shotsPerAttempt;
        [SerializeField] float attackDistanceMin;
        [SerializeField] float attackDistanceMax;
        [SerializeField] float kickDistance;
        [SerializeField] DuoInt damage;
        [SerializeField] float hitCooldown;
        [SerializeField] LayerMask targetLayer;
        [SerializeField] AnimationCurve yBombMovementCurve;

        public float KickDistance => kickDistance;
        public float AttackDistanceMin => attackDistanceMin;
        public float AttackDistanceMax => attackDistanceMax;
        public float HitCooldown => hitCooldown;

        [Space]
        [SerializeField] float bombDamageMin;
        [SerializeField] float bombDamageMax;
        [SerializeField] float bombExplosionDuration;
        [SerializeField] float bombExplosionRadius;
        [SerializeField] float bombShakeDurationMin;
        [SerializeField] float bombShakeDurationMax;
        [SerializeField] float bombShakeDistance;

        [Header("Weapon")]
        [SerializeField] Transform weaponHolderTransform;
        [SerializeField] Transform weaponTransform;
        [SerializeField] Transform shootPointTransform;
        [SerializeField] ParticleSystem shootParticleSystem;

        [Header("Boss")]
        [SerializeField] Transform leftFootTransform;
        [SerializeField] Transform rightFootTransform;
        [SerializeField] Transform backTransform;

        [Space]
        [SerializeField] Collider bossCollider;
        [SerializeField] WeaponRigBehavior weaponRigBehavior;

        float sqrSpeed;

        Pool bulletPool;

        Vector3 bombPoint;

        int shotsAmount;

        VirtualCameraCase mainCameraCase;

        public event SimpleCallback OnEntered;

        protected override void Awake()
        {
            base.Awake();

            bulletPool = new Pool(new PoolSettings(bulletPrefab.name, bulletPrefab, 3, true));

            bossCollider.enabled = false;
        }

        public override void Initialise()
        {
            base.Initialise();

            sqrSpeed = stats.MoveSpeed * stats.MoveSpeed;

            // Reset weapon position
            weaponTransform.SetParent(weaponHolderTransform);
            weaponTransform.ResetLocal();
            weaponTransform.gameObject.SetActive(true);

            isDead = true;

            mainCameraCase = CameraController.GetCamera(CameraType.Main);

            // Disable graphics
            graphicsObject.SetActive(false);
            healthbarBehaviour.HealthBarTransform.gameObject.SetActive(false);

            weaponRigBehavior.enabled = true;

            weaponTransform.gameObject.SetActive(false);
        }

        public void PerformHit()
        {
            navMeshAgent.SetDestination(transform.position);
            navMeshAgent.isStopped = true;
            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);

            transform.LookAt(target);

            navMeshAgent.isStopped = true;

            shotsAmount = shotsPerAttempt;

            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);
            animatorRef.SetBool(ANIMATOR_SHOOTING_HASH, true);
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);

            bombPoint = target.position;
        }

        public void PerformKick()
        {
            transform.LookAt(target);

            navMeshAgent.isStopped = true;

            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);
            animatorRef.SetBool(ANIMATOR_SHOOTING_HASH, false);
            animatorRef.SetTrigger(ANIMATOR_KICK_HASH);

            Tween.DelayedCall(0.3f, () =>
            {
                AudioController.Play(AudioController.Sounds.punch1);
            });
        }

        public void ChasePlayer()
        {
            navMeshAgent.SetDestination(target.position);
            navMeshAgent.isStopped = false;
            animatorRef.SetBool(ANIMATOR_RUN_HASH, true);
        }

        public void Idle()
        {
            navMeshAgent.SetDestination(transform.position);
            navMeshAgent.isStopped = true;
            animatorRef.SetBool(ANIMATOR_RUN_HASH, false);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!LevelController.IsGameplayActive)
                return;

            if (isDead)
                return;

            if (navMeshAgent.isStopped)
            {
                var targetRotation = Quaternion.LookRotation(target.position - transform.position);

                // Smoothly rotate towards the target point.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 50);
            }

            healthbarBehaviour.FollowUpdate();

            animatorRef.SetFloat(ANIMATOR_SPEED_HASH, navMeshAgent.velocity.sqrMagnitude / sqrSpeed);
        }

        public override void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead)
                return;

            base.TakeDamage(damage, projectilePosition, projectileDirection);

            if (hitAnimationTime < Time.time)
                HitAnimation(Random.Range(0, 2));
        }

        protected override void OnDeath()
        {
            if (isDead)
                return;

            isDead = true;

            StateMachine.StopMachine();

            navMeshAgent.enabled = false;

            // Disable healthbar
            healthbarBehaviour.DisableBar();

            animatorRef.Play(ANIMATOR_DIE_HASH, -1, 0);

            AudioController.Play(AudioController.Sounds.bossScream, 0.6f);
            DropResources();

            LevelController.OnEnemyKilled(this);

            OnDiedEvent?.Invoke(this);

            weaponRigBehavior.enabled = false;

            ActivateRagdollOnDeath();
        }

        public void OnBombExploded(BossBombBehaviour bossBombBehaviour, bool playerHitted)
        {
            if (playerHitted)
            {
                mainCameraCase.Shake(0.04f, 0.04f, bombShakeDurationMax, 1.4f);

                return;
            }

            var distanceMultiplier = 1.0f - Mathf.InverseLerp(0, bombShakeDistance, Vector3.Distance(characterBehaviour.transform.position, bossBombBehaviour.transform.position));
            if (distanceMultiplier != 0.0f)
                mainCameraCase.Shake(0.04f, 0.04f, Mathf.Lerp(bombShakeDurationMin, bombShakeDurationMax, distanceMultiplier), 1.4f);
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:

                    OnBossHit();
                    break;

                case EnemyCallbackType.BossKick:

                    if (Vector3.Distance(transform.position, target.position) <= kickDistance)
                    {
                        ParticlesController.Play(PARTICLE_KICK_HASH).SetPosition(leftFootTransform.position);

                        characterBehaviour.TakeDamage(characterBehaviour.MaxHealth * 0.5f);
                    }
                    break;

                case EnemyCallbackType.BossLeftStep:
                    ParticlesController.Play(PARTICLE_STEP_HASH).SetPosition(leftFootTransform.position);
                    break;

                case EnemyCallbackType.BossRightStep:
                    ParticlesController.Play(PARTICLE_STEP_HASH).SetPosition(rightFootTransform.position);
                    break;

                case EnemyCallbackType.BossEnterFall:

                    OnBossEnterFall();
                    break;

                case EnemyCallbackType.BossEnterFallFinished:

                    OnEntered?.Invoke();

                    CharacterBehaviour.GetBehaviour().TryAddClosestEnemy(this);
                    break;

                case EnemyCallbackType.BossDeathFall:
                    ParticlesController.Play(PARTICLE_DEATH_FALL_HASH).SetPosition(backTransform.position);

                    mainCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);
                    break;
            }
        }

        void OnBossHit()
        {
            shootParticleSystem.Play();

            var bombObject = bulletPool.Get();
            bombObject.transform.position = shootPointTransform.position;
            bombObject.transform.LookAt(bombPoint);

            var bossBombBehaviour = bombObject.GetComponent<BossBombBehaviour>();
            bossBombBehaviour.Initialise(this, bombExplosionDuration, Random.Range(bombDamageMin, bombDamageMax), bombExplosionRadius);

            bossBombBehaviour.transform.DOMoveXZ(bombPoint.x, bombPoint.z, 1.0f);
            bossBombBehaviour.transform.DOMoveY(bombPoint.y, 1.0f).SetCurveEasing(yBombMovementCurve).OnComplete(delegate
            {
                bossBombBehaviour.OnPlaced();
            });

            shotsAmount--;

            // Recalculate bomb point
            bombPoint = target.position + new Vector3(Random.Range(-4, 4), 0, Random.Range(-4, 4));

            if (shotsAmount <= 0)
            {
                // move closer to the player
                navMeshAgent.SetDestination(target.position);
                navMeshAgent.isStopped = false;
                animatorRef.SetBool(ANIMATOR_SHOOTING_HASH, false);
                animatorRef.SetBool(ANIMATOR_RUN_HASH, true);
            }

            AudioController.Play(AudioController.Sounds.shoot2, 0.3f);
        }

        void OnBossEnterFall()
        {
            isDead = false;

            healthbarBehaviour.HealthBarTransform.gameObject.SetActive(true);

            ParticlesController.Play(PARTICLE_ENTER_FALL_HASH).SetPosition(transform.position);

            mainCameraCase.Shake(0.04f, 0.04f, 0.3f, 1.4f);

            AudioController.Play(AudioController.Sounds.jumpLanding);

            CharacterBehaviour.GetBehaviour().TryAddClosestEnemy(this);

            weaponTransform.gameObject.SetActive(true);

            Tween.DelayedCall(0.4f, () =>
            {
                AudioController.Play(AudioController.Sounds.bossScream, 0.6f);
            });
        }

        public void Enter()
        {
            // Enable graphics
            graphicsObject.SetActive(true);
            healthbarBehaviour.HealthBarTransform.gameObject.SetActive(true);

            animatorRef.Play(ANIMATOR_ENTER_HASH, -1, 0);
        }

        public override void Attack()
        {

        }
    }
}