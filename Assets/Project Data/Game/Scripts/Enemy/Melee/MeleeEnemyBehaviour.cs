using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class MeleeEnemyBehaviour : BaseEnemyBehavior
    {
        static readonly int HIT_PARTICLE_HASH = ParticlesController.GetHash("Enemy Melee Hit");
        readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");
        public Material[] normalMaterials;
        public Material[] ninjaMaterials;

        [Header("Fighting")]
        [SerializeField] float hitRadius;
        [SerializeField] DuoFloat slowDownDuration;
        [SerializeField] float slowDownSpeedMult;

        [Space]
        [SerializeField] Transform hitParticlePosition;

        float slowRunningTimer;
        bool isHitting;
        bool isSlowRunning;

        [SerializeField] List<SkinnedMeshRenderer> meshes = new();
        [SerializeField] Material normalMaterial;
        [SerializeField] Material ninjaMaterial;
        [SerializeField] bool haveStealth;
        [SerializeField] float stealthDuration;
        [SerializeField] float stealthCooldown;
        float stealthCooldownTimer;
        float stealthDurationTimer;
  
       // [NaughtyAttributes.Button()]
        public void EnableStealth()
        {
            Debug.LogError("IS STEALTH");
            isInStealth = true;
            stealthCooldownTimer = stealthCooldown;
            stealthDurationTimer = stealthDuration;

            foreach (var mesh in meshes)
            {
                mesh.materials[0] = ninjaMaterial;
            }
        }

       // [NaughtyAttributes.Button()]
        public void DisableStealth()
        {
            Debug.LogError("NO STEALTH");
            isInStealth = false;

            foreach (var mesh in meshes)
            {
                mesh.materials = normalMaterials;
                // mesh.materials[0] = normalMaterial;
            }
        }

        public override void Attack()
        {
            if (isHitting) return;
            isHitting = true;
            ApplySlowDown();
            AudioController.Play(AudioController.Sounds.enemyMeleeHit, 0.5f);
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);
        }

        void ApplySlowDown()
        {
            isSlowRunning = true;
            IsWalking = true;
            slowRunningTimer = slowDownDuration.Random();
            navMeshAgent.speed = Stats.MoveSpeed * slowDownSpeedMult;
        }

        void DisableSlowDown()
        {
            isSlowRunning = false;
            IsWalking = false;
            navMeshAgent.speed = Stats.MoveSpeed;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isDead) return;
            if (!LevelController.IsGameplayActive) return;

            healthbarBehaviour.FollowUpdate();

            if (isSlowRunning)
            {
                slowRunningTimer -= Time.deltaTime;
                if (slowRunningTimer <= 0)
                    DisableSlowDown();
            }

            if (stunCooldownTimer > 0)
                stunCooldownTimer -= Time.fixedDeltaTime;

            if (stunCooldownTimer <= 0)
                isStunCharged = true;

            if (haveStealth)
            {
                if (isInStealth)
                {
                    stealthDurationTimer -= Time.fixedDeltaTime;
                    if (stealthDurationTimer <= 0)
                    {
                        DisableStealth();
                    }
                    else
                    {
                        Debug.LogError(("GOOOODDDDDDDDD"));
                        foreach (var mesh in meshes)
                        {
                            mesh.materials = ninjaMaterials;
                        }
                    }
                }
                else
                {
                    stealthCooldownTimer -= Time.fixedDeltaTime;
                    if (stealthCooldownTimer <= 0)
                    {
                        EnableStealth();
                    }
                }
            }

            OnFixedUpdate();
        }

        protected virtual void OnFixedUpdate()
        {
        }

        public override void TakeDamage(float damage, Vector3 projectilePosition, Vector3 projectileDirection)
        {
            if (isDead) return;
            base.TakeDamage(damage, projectilePosition, projectileDirection);
            if (hitAnimationTime < Time.time)
                HitAnimation(Random.Range(0, 2));
        }

        public float stunDuration = 1f;
        public float stunCooldown = 6f;
        float stunCooldownTimer;
        bool isStunCharged = true;
        public bool haveStun;

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (!target) Debug.LogError("NO TARGET", gameObject);

            switch (enemyCallbackType)
            {
                case EnemyCallbackType.Hit:
                {
                    if (Vector3.Distance(transform.position, target.position) <= hitRadius)
                    {
                        characterBehaviour.TakeDamage(Damage);
                        ParticlesController.Play(HIT_PARTICLE_HASH).SetPosition(hitParticlePosition.position);
                        OnAttackPlayer();
                        Stun();
                    }

                    break;
                }
                case EnemyCallbackType.HitFinish:
                    isHitting = false;
                    InvokeOnAttackFinished();
                    break;
            }
        }

        void Stun()
        {
            if (!haveStun) return;
            if (!isStunCharged) return;
            isStunCharged = false;

            characterBehaviour.ApplyStun(stunDuration);
            stunCooldownTimer = stunCooldown;
        }

        protected virtual void OnAttackPlayer()
        {
        }
    }
}