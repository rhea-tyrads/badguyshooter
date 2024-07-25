using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class TeslaBulletBehavior : PlayerBulletBehavior
    {
        static readonly int ParticleHitHash = ParticlesController.GetHash("Tesla Hit");
        static readonly int ParticleWallHitHash = ParticlesController.GetHash("Tesla Wall Hit");

        [Space(5f)]
        [SerializeField] TrailRenderer trailRenderer;

        List<BaseEnemyBehavior> _targets;

        int _targetsHitGoal;
        int _hitsPerformed;
        float _stunDuration;

        public void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime,
            bool autoDisableOnHit, float stunDuration)
        {
            base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            this._stunDuration = stunDuration;
            trailRenderer.Clear();

            transform.localScale = Vector3.one * 0.1f;
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn);

            _hitsPerformed = 0;
            _targets = ActiveRoom.GetAliveEnemies().OrderBy(e =>
                Vector3.SqrMagnitude(e.transform.position - CharacterBehaviour.Transform.position)).ToList();
        }

        public void SetTargetsHitGoal(int goal)
        {
            _targetsHitGoal = goal;
        }

        protected override void FixedUpdate()
        {
            if (_targets.Count == 0)
            {
                DisableBullet();
                return;
            }

            if (_hitsPerformed >= _targetsHitGoal)
            {
                DisableBullet();
                return;
            }


            var targetDirection = _targets[0].transform.position.SetY(1f) - transform.position;
            var rotationDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0f);
            transform.rotation = Quaternion.LookRotation(rotationDirection);

            base.FixedUpdate();

            if (_targets[0].IsDead)
            {
                _targets.RemoveAt(0);
            }
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            baseEnemyBehavior.Stun(_stunDuration);
            ParticlesController.Play(ParticleHitHash).SetPosition(transform.position);

            trailRenderer.Clear();

            for (var i = 0; i < _targets.Count; i++)
            {
                if (_targets[i].IsDead || _targets[i].Equals(baseEnemyBehavior))
                {
                    _targets.RemoveAt(i);
                    i--;
                }
            }

            _hitsPerformed++;

            // all hits after the first one deal 30% of damage
            if (_hitsPerformed == 1)
            {
                Damage *= 0.3f;
            }

            if (_hitsPerformed >= _targetsHitGoal || _targets.Count == 0)
            {
                DisableBullet();
            }
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            ParticlesController.Play(ParticleWallHitHash).SetPosition(transform.position);
            DisableBullet();
        }

        void DisableBullet()
        {
            trailRenderer.Clear();
            gameObject.SetActive(false);
        }
    }
}