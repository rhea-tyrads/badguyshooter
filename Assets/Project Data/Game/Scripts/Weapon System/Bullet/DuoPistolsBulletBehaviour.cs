using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class DuoPistolsBulletBehaviour : PlayerBulletBehavior
{
    static readonly int ParticleHitHash = ParticlesController.GetHash("Minigun Hit");
    static readonly int ParticleWAllHitHash = ParticlesController.GetHash("Minigun Wall Hit");
   public float knockBackForce = 1.5f;
    [SerializeField] TrailRenderer trailRenderer;
    public CharacterBehaviour owner;

    public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime,
        bool autoDisableOnHit = true)
    {
        hitted.Clear();
        base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);
        trailRenderer.Clear();
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
    {
        if (hitted.Count == 1)
        {
            var knockBackDir = (baseEnemyBehavior.Position - owner.transform.position).normalized;
            baseEnemyBehavior.KnockBack(knockBackDir, knockBackForce);
        }

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
