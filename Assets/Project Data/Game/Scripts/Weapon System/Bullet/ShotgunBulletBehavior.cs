using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class ShotgunBulletBehavior : PlayerBulletBehavior
    {
        static readonly int ParticleHitHash = ParticlesController.GetHash("Shotgun Hit");
        static readonly int ParticleWallHitHash = ParticlesController.GetHash("Shotgun Wall Hit");

        [SerializeField] TrailRenderer trailRenderer;
        [SerializeField] Transform graphicsTransform;

        public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime, bool autoDisableOnHit = true)
        {
            base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);

            trailRenderer.Clear();

            transform.localScale = Vector3.one * 0.1f;
            transform.DOScale(1.0f, 0.25f).SetEasing(Ease.Type.CubicIn);
        }

        protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
        {
            ParticlesController.Play(ParticleHitHash).SetPosition(transform.position);

            baseEnemyBehavior.Stun(0.1f);

            trailRenderer.Clear();
        }

        protected override void OnObstacleHitted()
        {
            base.OnObstacleHitted();

            ParticlesController.Play(ParticleWallHitHash).SetPosition(transform.position);
            trailRenderer.Clear();
        }
    }
}