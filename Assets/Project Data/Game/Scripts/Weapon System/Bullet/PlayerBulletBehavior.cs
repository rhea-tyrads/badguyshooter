using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // base class for player bullets
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public abstract class PlayerBulletBehavior : MonoBehaviour
    {
        protected float Damage;
        protected float Speed;
        bool _autoDisableOnHit;

        TweenCase _disableTweenCase;

        protected BaseEnemyBehavior CurrentTarget;

        public virtual void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget,
            float autoDisableTime, bool autoDisableOnHit = true)
        {
            hitted.Clear();
            this.Damage = damage;
            this.Speed = speed;
            this._autoDisableOnHit = autoDisableOnHit;

            this.CurrentTarget = currentTarget;

            if (autoDisableTime > 0)
            {
                _disableTweenCase = Tween.DelayedCall(autoDisableTime, delegate
                {
                    // Disable bullet
                    gameObject.SetActive(false);
                });
            }
        }

        protected virtual void FixedUpdate()
        {
            if (Speed != 0)
                transform.position += transform.forward * (Speed * Time.fixedDeltaTime);
        }

        public List<Transform> hitted = new();

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicsHelper.LAYER_ENEMY)
            {
                if (hitted.Contains(other.transform)) return;
                hitted.Add(other.transform);

                var baseEnemyBehavior = other.GetComponent<BaseEnemyBehavior>();
                if (baseEnemyBehavior == null) return;
                if (baseEnemyBehavior.IsDead) return;
                _disableTweenCase.KillActive();

                // Disable bullet
                if (_autoDisableOnHit)
                    gameObject.SetActive(false);

                // Deal damage to enemy
                baseEnemyBehavior.TakeDamage(CharacterBehaviour.NoDamage ? 0 : Damage, transform.position,
                    transform.forward);

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
            _disableTweenCase.KillActive();
        }

        void OnDestroy()
        {
            _disableTweenCase.KillActive();
        }

        protected abstract void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior);

        protected virtual void OnObstacleHitted()
        {
            _disableTweenCase.KillActive();

            gameObject.SetActive(false);
        }
    }
}