using Watermelon;
using Watermelon.SquadShooter;

public class FlamethrowerBulletBehaviour : PlayerBulletBehavior
{
    public override void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget, float lifeTime,
        bool disableOnHit = true)
    {
        base.Initialise(dmg, speed, currentTarget, lifeTime, disableOnHit);

    }

    protected override void OnEnemyHitted(BaseEnemyBehavior target)
    {
        target.ApplyDPS(Damage);
       
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
 
    }
}