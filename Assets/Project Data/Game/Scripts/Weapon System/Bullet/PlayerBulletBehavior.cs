using UnityEngine;

namespace Watermelon.SquadShooter
{
    // base class for player bullets
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public abstract class PlayerBulletBehavior : MonoBehaviour
    {
        protected float damage;
        protected float speed;
        bool autoDisableOnHit;

        TweenCase disableTweenCase;

        protected BaseEnemyBehavior currentTarget;

        public virtual void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            this.damage = damage;
            this.speed = speed;
            this.autoDisableOnHit = autoDisableOnHit;

            this.currentTarget = currentTarget;

            if (autoDisableTime > 0)
            {
                disableTweenCase = Tween.DelayedCall(autoDisableTime, delegate
                {
                    // Disable bullet
                    gameObject.SetActive(false);
                });
            }
        }

        protected virtual void FixedUpdate()
        {
            if (speed != 0)
                transform.position += transform.forward * (speed * Time.fixedDeltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                var baseEnemyBehavior = other.GetComponent<BaseEnemyBehavior>();
                if (baseEnemyBehavior == null) return;
                if (baseEnemyBehavior.IsDead) return;
                disableTweenCase.KillActive();

                // Disable bullet
                if (autoDisableOnHit)
                    gameObject.SetActive(false);

                // Deal damage to enemy
                baseEnemyBehavior.TakeDamage(CharacterBehaviour.NoDamage ? 0 : damage, transform.position, transform.forward);

                // Call hit callback
                OnEnemyHitted(baseEnemyBehavior);
            }
            else
            {
                OnObstacleHitted();
            }
        }

        void OnDisable()
        {
            disableTweenCase.KillActive();
        }

        void OnDestroy()
        {
            disableTweenCase.KillActive();
        }

        protected abstract void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior);

        protected virtual void OnObstacleHitted()
        {
            disableTweenCase.KillActive();

            gameObject.SetActive(false);
        }
    }
}