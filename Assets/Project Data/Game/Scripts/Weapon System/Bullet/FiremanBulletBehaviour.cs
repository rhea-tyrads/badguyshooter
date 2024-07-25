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

    bool _inactive;

    void OnTriggerEnter(Collider other)
    {
        if (_inactive) return;
        if (other.gameObject.layer != PhysicsHelper.LAYER_ENEMY) return;
        
        var baseEnemyBehavior = other.GetComponent<BaseEnemyBehavior>();
        Damage(baseEnemyBehavior);
    }

    void Damage(BaseEnemyBehavior enemy)
    {
        if (enemy == null) return;
        if (enemy.IsDead) return;
        _inactive = true;
        enemy.TakeDamage(CharacterBehaviour.NoDamage ? 0 : damage, transform.position, transform.forward);
        container.SetActive(false);
        hitVfx.gameObject.SetActive(true);
        Invoke(nameof(Activate), cooldown);
        trailRenderer.Clear();
    }

    void Activate()
    {
        _inactive = false;
        container.SetActive(true);
    }
}