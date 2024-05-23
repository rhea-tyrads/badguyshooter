using System;
using UnityEngine;
using UnityEngine.UI;

public class BonusUI : MonoBehaviour
{
    public Button playButton;
    public BonusSlot hpBonus;
    public BonusSlot critBonus;
    public BonusSlot respawnBonus;
    public Canvas canvas;
    public GraphicRaycaster canvasCast;

    
    public bool IsHpActive;
    public bool IsCritActive;
    public bool IsRespawnActive;
    BonusController c;
    public   event Action OnPlay = delegate { };


    void Awake()
    {
        BonusController.OnShow += Show;

        playButton.onClick.AddListener(Play);

        hpBonus.OnClick += SwitchHP;
        critBonus.OnClick += SwitchCrit;
        respawnBonus.OnClick += SwitchRespawn;
        
        hpBonus.Disable();
        critBonus.Disable();
        respawnBonus.Disable();
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
        IsHpActive = !IsHpActive;
        if (IsHpActive) hpBonus.Activate();
        else hpBonus.Disable();
    }

    void SwitchCrit()
    {
        IsCritActive = !IsCritActive;
        if (IsCritActive) critBonus.Activate();
        else critBonus.Disable();
    }

    void SwitchRespawn()
    {
        IsRespawnActive = !IsRespawnActive;
        if (IsRespawnActive) respawnBonus.Activate();
        else respawnBonus.Disable();
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
}