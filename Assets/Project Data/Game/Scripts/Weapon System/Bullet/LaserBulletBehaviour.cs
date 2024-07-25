using Watermelon;
using Watermelon.SquadShooter;

public class LaserBulletBehaviour : PlayerBulletBehavior
{
    public override void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget, float lifeTime,
        bool disableOnHit = true)
    {
        base.Initialise(dmg, speed, currentTarget, lifeTime, disableOnHit);
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior target)
    {
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
    }
}