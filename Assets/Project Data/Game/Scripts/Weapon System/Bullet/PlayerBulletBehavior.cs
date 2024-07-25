using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    // base class for player bullets
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public abstract class PlayerBulletBehavior : MonoBehaviour
    {
        [SerializeField] string hitVfx;
        [SerializeField] string wallHitVfx;

        int _hitVfx  ;
        int _hitWallVfx  ;
        protected float Damage;
        protected float Speed;
        bool _autoDisableOnHit;

        TweenCase _disableTweenCase;

        protected BaseEnemyBehavior CurrentTarget;

        public virtual void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget,
            float lifeTime, bool disableOnHit = true)
        {
            hitted.Clear();
            Damage = dmg;
            Speed = speed;
            _autoDisableOnHit = disableOnHit;
            CurrentTarget = currentTarget;

            _hitVfx = ParticlesController.GetHash(hitVfx);
            _hitWallVfx = ParticlesController.GetHash(wallHitVfx);


            if (lifeTime > 0)
            {
                _disableTweenCase = Tween.DelayedCall(lifeTime, delegate
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

                if (_autoDisableOnHit)
                    gameObject.SetActive(false);

                baseEnemyBehavior.TakeDamage(CharacterBehaviour.NoDamage ? 0 : Damage, transform.position,
                    transform.forward);

                ParticlesController.Play(_hitVfx).SetPosition(transform.position);
                OnEnemyHitted(baseEnemyBehavior);
            }
            else
            {
                ParticlesController.Play(_hitWallVfx).SetPosition(transform.position);
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

        protected abstract void OnEnemyHitted(BaseEnemyBehavior target);

        protected virtual void OnObstacleHitted()
        {
            _disableTweenCase.KillActive();

            gameObject.SetActive(false);
        }
    }
}