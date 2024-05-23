using System;
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

public class BonusController : MonoBehaviour
{
    public BonusFile file;
    public BonusUI ui;
    const string KEY = "BONUSES_KEY";

    void Start()
    {
        Load();

        file.critBonus = 3;
        file.hpBonus = 3;
        file.respawnBonus = 3;

        ui.SetCritBonus(file.critBonus);
        ui.SetHpBonus(file.hpBonus);
        ui.SetRespawnBonus(file.respawnBonus);

        ui.OnPlay += SetBonuses;
    }

    public static event Action OnShow = delegate { };

    public static void Show()
    {
        OnShow();
    }

    void SetBonuses()
    {
        if (ui.IsHpActive)
        {
            LevelController.characterBehaviour.ApplyHitpointsBonus();
        }

        if (ui.IsCritActive)
        {
            LevelController.characterBehaviour.ApplyCriticalBonus();
        }

        if (ui.IsRespawnActive)
        {
            LevelController.characterBehaviour.ApplyRespawnBonus();
        }
        
   
        LevelController.StartGame();
    }

    public    void Hide() => ui.Hide();

    public void AddHp()
    {
        file.hpBonus++;
        Save();
    }

    public void AddRespawn()
    {
        file.respawnBonus++;
        Save();
    }

    public void AddCrit()
    {
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