using System;
using UnityEngine;
using UnityEngine.UI;

public class BonusUI : MonoBehaviour
{
    public bool stackMode;

    public Button playButton;
    public BonusSlot hpBonus;
    public BonusSlot critBonus;
    public BonusSlot respawnBonus;
    public BonusSlot firerateBonus;
    public BonusSlot multishotBonus;
    public BonusSlot movementBonus;
    public Canvas canvas;
    public GraphicRaycaster canvasCast;

    public bool IsHpActive;
    public bool IsCritActive;
    public bool IsRespawnActive;
    public bool IsMovementActive;
    public bool IsFirerateActive;
    public bool IsMultishotActive;

    BonusController c;

    public event Action OnPlay = delegate { };


    void Awake()
    {
        BonusController.OnShow += Show;

        playButton.onClick.AddListener(Play);
        hpBonus.OnClick += SwitchHP;
        critBonus.OnClick += SwitchCrit;
        respawnBonus.OnClick += SwitchRespawn;
        movementBonus.OnClick += SwitchMovement;
        multishotBonus.OnClick += SwitchMultishot;
        firerateBonus.OnClick += SwitchFireraet;

        hpBonus.Disable();
        critBonus.Disable();
        respawnBonus.Disable();
        movementBonus.Disable();
        multishotBonus.Disable();
        firerateBonus.Disable();
    }

    void Show()
    {
        canvas.enabled = true;
        canvasCast.enabled = true;
    }

    void Play()
    {
        Hide();
        OnPlay();
    }

    public void Hide()
    {
        canvas.enabled = false;
        canvasCast.enabled = false;
    }

    void SwitchHP()
    {
        if (stackMode)
        {
            hpBonus.Increment();
        }
        else
        {
            IsHpActive = !IsHpActive;
            if (IsHpActive) hpBonus.Activate();
            else hpBonus.Disable();
        }
    }

    void SwitchCrit()
    {
        if (stackMode)
            critBonus.Increment();
        else
        {
            IsCritActive = !IsCritActive;
            if (IsCritActive) critBonus.Activate();
            else critBonus.Disable();
        }
    }

    void SwitchRespawn()
    {
        if (stackMode)
            respawnBonus.Increment();
        else
        {
            IsRespawnActive = !IsRespawnActive;
            if (IsRespawnActive) respawnBonus.Activate();
            else respawnBonus.Disable();
        }
    }

    void SwitchMovement()
    {
        if (stackMode)
            movementBonus.Increment();
        else
        {
            IsMovementActive = !IsMovementActive;
            if (IsMovementActive) movementBonus.Activate();
            else movementBonus.Disable();
        }
    }

    void SwitchFireraet()
    {
        if (stackMode)
            firerateBonus.Increment();
        else
        {
            IsFirerateActive = !IsFirerateActive;
            if (IsFirerateActive) firerateBonus.Activate();
            else firerateBonus.Disable();  
        }

    }

    void SwitchMultishot()
    {
        if (stackMode)
            multishotBonus.Increment();
        else
        {
            IsMultishotActive = !IsMultishotActive;
            if (IsMultishotActive) multishotBonus.Activate();
            else multishotBonus.Disable();
        }

    }


    public void SetHpBonus(int amount)
    {
        hpBonus.SetAmount(amount);
    }

    public void SetCritBonus(int amount)
    {
        critBonus.SetAmount(amount);
    }

    public void SetRespawnBonus(int amount)
    {
        respawnBonus.SetAmount(amount);
    }

    public void SetOtherBonuses()
    {
        firerateBonus.SetAmount(999);
        multishotBonus.SetAmount(999);
        movementBonus.SetAmount(999);
    }
}