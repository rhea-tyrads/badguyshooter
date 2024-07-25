using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class DuoPistolsBulletBehaviour : PlayerBulletBehavior
{
   public float knockBackForce = 1.5f;
    [SerializeField] TrailRenderer trailRenderer;
    public CharacterBehaviour owner;

    public override void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget, float lifeTime,
        bool disableOnHit = true)
    {
        hitted.Clear();
        base.Initialise(dmg, speed, currentTarget, lifeTime, disableOnHit);
        trailRenderer.Clear();
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior target)
    {
        if (hitted.Count == 1)
        {
            var knockBackDir = (target.Position - owner.transform.position).normalized;
            target.KnockBack(knockBackDir, knockBackForce);
        }

        trailRenderer.Clear();
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
        trailRenderer.Clear();
    }
}
