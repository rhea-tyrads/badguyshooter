using System;
using MobileTools.MonoCache.System;
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

public class BonusController : Singleton<BonusController>
{
    public BonusFile file;
    public BonusUI ui;
    const string KEY = "BONUSES_SAVE";

    bool IsFirstLaunch
    {
        get
        {
            var key = "FirstLaunchBonus";
            if (PlayerPrefs.HasKey(key)) return false;
            PlayerPrefs.SetInt(key, 1);
            return true;
        }
    }

    void Awake()
    {
        Load();

        if (IsFirstLaunch)
        {
            file.critBonus = 999;
            file.hpBonus =  999;
            file.respawnBonus =  999;
            Save();
        }

        ui.SetCritBonus(file.critBonus);
        ui.SetHpBonus(file.hpBonus);
        ui.SetRespawnBonus(file.respawnBonus);
        ui.SetOtherBonuses();

        ui.OnPlay += UseBonuses;
    }

    public static event Action OnShow = delegate { };

    public static void Show()
    {
        OnShow();
    }

    public UIPowerupPanel powerups;
    void UseBonuses()
    {
        if (ui.IsHpActive)
        {
            LevelController.characterBehaviour.ApplyHitpointsBonus();
            file.hpBonus--;
        }

        if (ui.IsCritActive)
        {
            LevelController.characterBehaviour.ApplyCriticalBonus();
            file.critBonus--;
        }

        if (ui.IsRespawnActive)
        {
            LevelController.characterBehaviour.ApplyRespawnBonus();
            file.respawnBonus--;
        }

        if (ui.IsMovementActive)
        {
            LevelController.characterBehaviour.MovementBonus(true);
            file.movementBonus--;
        }
        
        if (ui.IsFirerateActive)
        {
            LevelController.characterBehaviour.FireRateBonus(true);
            file.fireRate--;
        }
        
        if (ui.IsMultishotActive)
        {
            LevelController.characterBehaviour.MultishotBonus(true);
            file.multishot--;
        }
        
        Save();

        powerups.SetBonuses(ui);
        LevelController.StartGame();
    }

    public void Hide() => ui.Hide();

    public void AddHp(int amount = 1)
    {
        if (amount == 0) return;
        for (var i = 0; i < amount; i++)
            file.hpBonus++;
        Save();
    }

    public void AddRespawn(int amount = 1)
    {
        if (amount == 0) return;
        for (var i = 0; i < amount; i++)
            file.respawnBonus++;
        Save();
    }

    public void AddCrit(int amount = 1)
    {
        if (amount == 0) return;
        for (var i = 0; i < amount; i++)
            file.critBonus++;
        Save();
    }

    void FindUI()
    {
        if (!ui) ui = FindObjectOfType<BonusUI>();
    }

    void Save()
        => PlayerPrefs.SetString(KEY, JsonUtility.ToJson(file));

    void Load() =>
        file = PlayerPrefs.HasKey(KEY)
            ? JsonUtility.FromJson<BonusFile>(PlayerPrefs.GetString(KEY, string.Empty))
            : new BonusFile();
}