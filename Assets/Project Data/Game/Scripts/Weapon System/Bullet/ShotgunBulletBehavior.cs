using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShotgunBulletBehavior : PlayerBulletBehavior
    {
        [SerializeField] TrailRenderer trailRenderer;
        [SerializeField] Transform graphicsTransform;

        public override void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget, float lifeTime, bool disableOnHit = true)
        {
            base.Initialise(dmg, speed, currentTarget, lifeTime, disableOnHit);
            trailRenderer.Clear();
            transform.localScale = Vector3.one * 0.1f;
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn);
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior target)
        {
            target.Stun(0.1f);
            trailRenderer.Clear();
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();
            trailRenderer.Clear();
        }
    }
}