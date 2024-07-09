using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class LaserBulletBehaviour : PlayerBulletBehavior
{
    static readonly int PARTICLE_HIT_HASH = ParticlesController.GetHash("Minigun Hit");
    static readonly int PARTICLE_WAll_HIT_HASH = ParticlesController.GetHash("Minigun Wall Hit");


    public CharacterBehaviour owner;

    public override void Initialise(float damage, float speed, BaseEnemyBehavior currentTarget, float autoDisableTime,
        bool autoDisableOnHit = true)
    {
        base.Initialise(damage, speed, currentTarget, autoDisableTime, autoDisableOnHit);
    }

    protected override void OnEnemyHitted(BaseEnemyBehavior baseEnemyBehavior)
    {
        ParticlesController.PlayParticle(PARTICLE_HIT_HASH).SetPosition(transform.position);
    }

    protected override void OnObstacleHitted()
    {
        base.OnObstacleHitted();
        ParticlesController.PlayParticle(PARTICLE_WAll_HIT_HASH).SetPosition(transform.position);
    }
}