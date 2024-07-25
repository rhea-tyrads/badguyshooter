using Watermelon;
using Watermelon.SquadShooter;

public class LaserBulletBehaviour : PlayerBulletBehavior
{
    static readonly int ParticleHitHash = ParticlesController.GetHash("Minigun Hit");
    static readonly int ParticleWAllHitHash = ParticlesController.GetHash("Minigun Wall Hit");


    public CharacterBehaviour owner;

    public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime,
        bool autoDisableOnHit = true)
    {
        base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
    {
        ParticlesController.Play(ParticleHitHash).SetPosition(transform.position);
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
        ParticlesController.Play(ParticleWAllHitHash).SetPosition(transform.position);
    }
}