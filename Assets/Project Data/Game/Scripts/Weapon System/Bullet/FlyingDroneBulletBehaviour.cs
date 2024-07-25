using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class FlyingDroneBulletBehaviour : PlayerBulletBehavior
{
    [SerializeField] TrailRenderer trailRenderer;

    public override void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget, float lifeTime,
        bool disableOnHit = true)
    {
        base.Initialise(dmg, speed, currentTarget, lifeTime, disableOnHit);
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