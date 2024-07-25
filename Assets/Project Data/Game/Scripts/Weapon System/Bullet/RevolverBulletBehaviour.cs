using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class RevolverBulletBehaviour : PlayerBulletBehavior
{
    public float knockBackForce = 1;
    [SerializeField] TrailRenderer trailRenderer;
    public CharacterBehaviour owner;

    public override void Initialise(float dmg, float speed, BaseEnemyBehavior currentTarget, float lifeTime,
        bool disableOnHit = true)
    {
        base.Initialise(dmg, speed, currentTarget, lifeTime, disableOnHit);
        trailRenderer.Clear();
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior target)
    {
       // var knockBackDir = (baseEnemyBehavior.Position - owner.transform.position).normalized;
      //  baseEnemyBehavior.KnockBack(knockBackDir, knockBackForce);
        trailRenderer.Clear();
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
        trailRenderer.Clear();
    }
}