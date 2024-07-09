using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.SquadShooter;

public class FiremanBulletBehaviour : MonoBehaviour
{
    public float damage;
    public float cooldown;
    public GameObject container;
    public ParticleSystem hitVfx;
    [SerializeField] TrailRenderer trailRenderer;

    void Start()
    {
        hitVfx.gameObject.SetActive(false);
    }

    bool Inactive;

    void OnTriggerEnter(Collider other)
    {
        if (Inactive) return;
        if (other.gameObject.layer != PhysicsHelper.LAYER_ENEMY) return;
        
        var baseEnemyBehavior = other.GetComponent<BaseEnemyBehavior>();
        Damage(baseEnemyBehavior);
    }

    void Damage(BaseEnemyBehavior enemy)
    {
        if (enemy == null) return;
        if (enemy.IsDead) return;
        Inactive = true;
        enemy.TakeDamage(CharacterBehaviour.NoDamage ? 0 : damage, transform.position, transform.forward);
        container.SetActive(false);
        hitVfx.gameObject.SetActive(true);
        Invoke(nameof(Activate), cooldown);
        trailRenderer.Clear();
    }

    void Activate()
    {
        Inactive = false;
        container.SetActive(true);
    }
}