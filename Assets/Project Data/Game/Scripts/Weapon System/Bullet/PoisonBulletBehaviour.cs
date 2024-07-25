using UnityEngine;
using UnityEngine.Serialization;
using Watermelon.SquadShooter;

public class PoisonBulletBehaviour : PlayerBulletBehavior
{
    [SerializeField] float movementSlow = 0.65f;
    [SerializeField] TrailRenderer trailRenderer;
    
    public override void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget, float lifeTime,
        bool disableOnHit = true)
    {
        base.Initialise(dmg, speed, currentTarget, lifeTime, disableOnHit);
        trailRenderer.Clear();
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior target)
    {
        target.ApplyMovementSlow(movementSlow);
        trailRenderer.Clear();
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
        trailRenderer.Clear();
    }
}
