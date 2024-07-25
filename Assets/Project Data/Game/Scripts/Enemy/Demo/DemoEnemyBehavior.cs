using System.Collections.Generic;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class DemoEnemyBehavior : BaseEnemyBehavior
    {
        static readonly int ANIMATOR_ATTACK_HASH = Animator.StringToHash("Attack");

        [SerializeField] float explosionRadius;
        [SerializeField] GameObject explosionCircle;
        [SerializeField] Transform bombBone;
        [SerializeField] GameObject bombObj;
        [SerializeField] GameObject fuseObj;

        [Space]
        [SerializeField] WeaponRigBehavior weaponRigBehavior;

        TweenCase explosionRadiusScaleCase;
        bool exploded = false;

        int explosionParticleHash;
        int explosionDecalParticleHash;

        protected override void Awake()
        {
            base.Awake();

            explosionParticleHash = ParticlesController.GetHash("Bomber Explosion");
            explosionDecalParticleHash = ParticlesController.GetHash("Bomber Explosion Decal");

            CanPursue = true;

            explosionCircle.gameObject.SetActive(false);
        }

        public override void Initialise()
        {
            base.Initialise();

            weaponRigBehavior.enabled = true;

            fuseObj.SetActive(false);
        }

        public override void OnAnimatorCallback(EnemyCallbackType enemyCallbackType)
        {
            if (enemyCallbackType != EnemyCallbackType.HitFinish) return;
            var particleCase = ParticlesController.Play(explosionParticleHash);

            particleCase.SetPosition(bombBone.position.SetY(0.1f));
            particleCase.SetDuration(1f);

            var decalCase = ParticlesController.Play(explosionDecalParticleHash).SetRotation(Quaternion.Euler(-90, 0, 0)).SetScale((10.0f).ToVector3());

            decalCase.SetPosition(transform.position);
            decalCase.SetDuration(5f);

            bombObj.gameObject.SetActive(false);

            if (Vector3.Distance(transform.position, Target.position) <= explosionRadius)
            {
                characterBehaviour.TakeDamage(Damage);
            }

            var aliveEnemies = ActiveRoom.GetAliveEnemies();

            foreach (var enemy in aliveEnemies)
            {
                if (enemy == this) continue;

                if (!(Vector3.Distance(transform.position, enemy.transform.position) <= explosionRadius)) continue;
                var bombPos = bombObj.transform.position;
                var direction = (enemy.transform.position.SetY(0) - bombPos.SetY(0)).normalized;

                enemy.TakeDamage(Damage, bombPos, direction);
            }

            explosionCircle.gameObject.SetActive(false);
            exploded = true;
            AudioController.Play(AudioController.Sounds.explode);
            OnDeath();
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (!LevelController.IsGameplayActive) return;

            healthbarBehaviour.FollowUpdate();
        }

        public void LightUpFuse()
        {
            fuseObj.SetActive(true);
        }

        public override void Attack()
        {
            animatorRef.SetTrigger(ANIMATOR_ATTACK_HASH);
            navMeshAgent.speed = 0;
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
            CanPursue = false;
            CanMove = false;

            explosionCircle.gameObject.SetActive(true);

            explosionCircle.transform.localScale = new Vector3(0f, 0.2f, 0f);
            explosionCircle.transform.DOScale(new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f), 1.66f).SetEasing(Ease.Type.QuadOut);
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            explosionRadiusScaleCase.KillActive();
            explosionCircle.gameObject.SetActive(false);

            fuseObj.SetActive(false);

            if (exploded)
            {
                ragdollCase.KillActive();
            }
            else
            {
                weaponRigBehavior.enabled = false;
            }
        }
    }
}