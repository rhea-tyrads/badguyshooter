using UnityEngine;
 
using Watermelon.SquadShooter;

public class JuggernautEnemyBehaviour : MeleeEnemyBehaviour
{
    public float stunDuration = 1f;
    public float stunCooldown = 6f;
    float stunCooldownTimer;
    bool isStunCharged = true;
 
    protected override void OnFixedUpdate()
    {
        if (stunCooldownTimer > 0)
            stunCooldownTimer -= Time.fixedDeltaTime;

        if (stunCooldownTimer <= 0)
            isStunCharged = true;
    }
    protected override void OnAttackPlayer()
    {
       
        Stun();    
    }
    
    void Stun()
    {
        if (!isStunCharged) return;
        isStunCharged = false;

        characterBehaviour.ApplyStun(stunDuration);
        stunCooldownTimer = stunCooldown;
    }
    
}