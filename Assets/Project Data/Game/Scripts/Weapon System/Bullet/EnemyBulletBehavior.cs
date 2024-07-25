using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyBulletBehavior : MonoBehaviour
    {
        [SerializeField] TrailRenderer trailRenderer;

        static readonly int ParticleHitHash = ParticlesController.GetHash("Shotgun Hit");
        static readonly int ParticleWallHitHash = ParticlesController.GetHash("Shotgun Wall Hit");

        protected float Damage;
        protected float Speed;

        protected float SelfDestroyDistance;
        protected float DistanceTraveled = 0;

        protected TweenCase DisableTweenCase;

        public virtual void Initialise(float damage, float speed, float selfDestroyDistance)
        {
            this.Damage = damage;
            this.Speed = speed;

            this.SelfDestroyDistance = selfDestroyDistance;
            DistanceTraveled = 0;

            trailRenderer.Clear();
            var time = trailRenderer.time;
            trailRenderer.time = 0;

            gameObject.SetActive(true);
            Tween.NextFrame(() =>
            {
                trailRenderer.Clear();
                trailRenderer.gameObject.SetActive(true);
                trailRenderer.Clear();

                trailRenderer.time = time;
            });
        }

        protected virtual void FixedUpdate()
        {
            transform.position += transform.forward * Speed * Time.fixedDeltaTime;

            if (SelfDestroyDistance != -1)
            {
                DistanceTraveled += Speed * Time.fixedDeltaTime;

                if (DistanceTraveled >= SelfDestroyDistance)
                {
                    SelfDestroy();
                }
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_PLAYER)
            {
                var characterBehaviour = other.GetComponent<CharacterBehaviour>();
                if (characterBehaviour != null)
                {
                    // Deal damage to enemy
                    characterBehaviour.TakeDamage(Damage);

                    SelfDestroy();
                }

                ParticlesController.Play(ParticleHitHash).SetPosition(transform.position);
            }
            else if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                SelfDestroy();

                ParticlesController.Play(ParticleWallHitHash).SetPosition(transform.position);
            }
        }

        public void SelfDestroy()
        {
            // Disable bullet
            trailRenderer.Clear();
            gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }
    }
}