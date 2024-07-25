using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class EnemyBulletBehavior : MonoBehaviour
    {
        [SerializeField] TrailRenderer trailRenderer;

        static readonly int _particleHitHash = ParticlesController.GetHash("Shotgun Hit");
        static readonly int _particleWallHitHash = ParticlesController.GetHash("Shotgun Wall Hit");

        protected float Damage;
        protected float Speed;

        float _selfDestroyDistance;
        float _distanceTraveled = 0;

        protected TweenCase DisableTweenCase;

        public virtual void Initialise(float damage, float speed, float selfDestroyDistance)
        {
            Damage = damage;
            Speed = speed;

            _selfDestroyDistance = selfDestroyDistance;
            _distanceTraveled = 0;

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
            transform.position += transform.forward * (Speed * Time.fixedDeltaTime);

            if (_selfDestroyDistance == -1) return;
            _distanceTraveled += Speed * Time.fixedDeltaTime;

            if (_distanceTraveled >= _selfDestroyDistance)
                SelfDestroy();
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

                ParticlesController.Play(_particleHitHash).SetPosition(transform.position);
            }
            else if (other.gameObject.layer == PhysicsHelper.LAYER_OBSTACLE)
            {
                SelfDestroy();
                ParticlesController.Play(_particleWallHitHash).SetPosition(transform.position);
            }
        }

        protected void SelfDestroy()
        {
            trailRenderer.Clear();
            gameObject.SetActive(false);
            trailRenderer.gameObject.SetActive(false);
        }
    }
}