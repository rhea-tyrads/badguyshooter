using UnityEngine;
using Watermelon.SquadShooter;

public class CharacterAbilityShield : MonoBehaviour
{
    public float cooldown;
    public float invincibleDuration = 0.5f;
    public int maxShield = 3;
    public ShieldAbilityUI shieldUI;
    
    int _currentShield;
    public GameObject model;
    public GameObject hitVfx;

    void Start()
    {
        shieldUI.Unparent();
        shieldUI.Follow(transform);
        Invoke(nameof(Subscribe), 0.1f);
    }

    void Subscribe()
    {
        CharacterBehaviour.DamageToShieldImmune += Damaged;
        Activate();
    }

    void Damaged()
    {
        if (_invincible) return;
        _invincible = true;
        Invoke(nameof(FinishInvincible), invincibleDuration);

        _currentShield--;
        shieldUI.Set(_currentShield);
        if (_currentShield <= 0)
        {
            Invoke(nameof(Deactivate),invincibleDuration);
        }

        PlayDmgVfx();
    }

    void FinishInvincible() => _invincible = false;
    private bool _invincible;

    void PlayDmgVfx()
    {
        hitVfx.SetActive(true);
    }

    void Activate()
    {
        _currentShield = maxShield;
        shieldUI.Set(_currentShield);
        shieldUI.gameObject.SetActive(true);
        CharacterBehaviour.IsShieldImmune = true;
        model.SetActive(true);
    }

    void Deactivate()
    {
        _currentShield = 0;
        CharacterBehaviour.IsShieldImmune = false;
        model.SetActive(false);
        shieldUI.gameObject.SetActive(false);
        Invoke(nameof(Activate), cooldown);
    }
}