using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class BossSniperBulletBehavior : EnemyBulletBehavior
    {
        static readonly int _particleWAllHitHash = ParticlesController.GetHash("Minigun Wall Hit");
        [SerializeField] LayerMask collisionLayer;
        List<Vector3> _hitPoints;
        int _nextHitPointId;
        Vector3 NextHitPoint => _hitPoints[_nextHitPointId];

        public void InitialiseBullet(float damage, float speed, float selfDestroyDistance, List<Vector3> hitPoints)
        {
            Initialise(damage, speed, selfDestroyDistance);

            _hitPoints = new List<Vector3>(hitPoints.ToArray());
            _nextHitPointId = 0;
        }

        protected override void FixedUpdate()
        {
            var distanceTraveledDuringThisFrame = Speed * Time.fixedDeltaTime;
            var distanceToNextHitPoint = (NextHitPoint - transform.position).magnitude;

            if (distanceTraveledDuringThisFrame > distanceToNextHitPoint)
            {
                transform.position = NextHitPoint;
                _nextHitPointId++;
                if (_nextHitPointId >= _hitPoints.Count)
                {
                    ParticlesController.Play(_particleWAllHitHash).SetPosition(transform.position);
                    SelfDestroy();
                }
                else
                {
                    ParticlesController.Play(_particleWAllHitHash).SetPosition(transform.position);
                    transform.forward = (NextHitPoint - transform.position).normalized;
                }
            }
            else
            {
                var directionToNextHitPoint = (NextHitPoint - transform.position).normalized;
                transform.position += directionToNextHitPoint * distanceTraveledDuringThisFrame;
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != PhysicsHelper.LAYER_PLAYER) return;
            var character = other.GetComponent<CharacterBehaviour>();
            if (character == null) return;
            character.TakeDamage(Damage);
            SelfDestroy();
        }
    }
}