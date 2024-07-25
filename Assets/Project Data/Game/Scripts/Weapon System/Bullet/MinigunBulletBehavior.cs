using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class MinigunBulletBehavior : PlayerBulletBehavior
    {
        [SerializeField] TrailRenderer trailRenderer;

        public override void Initialise(float dmg, float speed, BaseEnemyBehavior target, float lifeTime,
            bool disableOnHit = true)
        {
            base.Initialise(dmg, speed, target, lifeTime, disableOnHit);
            trailRenderer.Clear();
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior target)
        {
            trailRenderer.Clear();
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();
            trailRenderer.Clear();
        }
    }
}