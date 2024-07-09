 
using UnityEngine;
using Watermelon.SquadShooter;

public class CharacterAbilityShield : MonoBehaviour
{
    public float cooldown;

    public int maxShield = 3;

    int _currentShield;
    public GameObject model;
    public GameObject hitVfx;

    void Start()
    {
        Invoke(nameof(Subscribe), 0.1f);
    }

    void Subscribe()
    {
        CharacterBehaviour.OnDamageToShieldImmune += Damaged;
        Activate();
    }

    void Damaged()
    {
        _currentShield--;
        if (_currentShield <= 0)
        {
            Deactivate();
        }

        PlayDmgVfx();
    }

    void PlayDmgVfx()
    {
        hitVfx.SetActive(true);
    }

    void Activate()
    {
        _currentShield = maxShield;
        CharacterBehaviour.IsShieldImmune = true;
        model.SetActive(true);
    }

    void Deactivate()
    {
        _currentShield = 0;
        CharacterBehaviour.IsShieldImmune = false;
        model.SetActive(false);
        Invoke(nameof(Activate), cooldown);
    }
}