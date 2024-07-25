using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class PoisonBulletBehaviour : PlayerBulletBehavior
{
 
    const float MOVEMENT_SLOW_FACTOR = 0.65f;
    [SerializeField] TrailRenderer trailRenderer;
    public CharacterBehaviour owner;
    static readonly int ParticleHitHash = ParticlesController.GetHash("Minigun Hit");
    static readonly int ParticleWAllHitHash = ParticlesController.GetHash("Minigun Wall Hit");
    
    public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime,
        bool autoDisableOnHit = true)
    {
        base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);
        trailRenderer.Clear();
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
    {
        baseEnemyBehavior.ApplyMovementSlow(MOVEMENT_SLOW_FACTOR);
        ParticlesController.Play(ParticleHitHash).SetPosition(transform.position);
        trailRenderer.Clear();
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
        ParticlesController.Play(ParticleWAllHitHash).SetPosition(transform.position);
        trailRenderer.Clear();
    }
}
