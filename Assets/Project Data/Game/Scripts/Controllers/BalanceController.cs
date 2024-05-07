using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class BalanceController : MonoBehaviour
    {
        [SerializeField] bool showDevText = true;
        [SerializeField] bool disableDifficulty = false;

        [SerializeField] List<DifficultySettings> difficultyPresets;

        TextMeshProUGUI devText;

        static BalanceController instance;

        public static Difficulty CurrentDifficulty { get; private set; }
        public static int PowerRequirement { get; private set; }
        public static int CurrentGeneralPower => CharactersController.SelectedCharacter.GetCurrentUpgrade().Stats.Power + (WeaponsController.GetCurrentWeaponUpgrade().CurrentStage as BaseWeaponUpgradeStage).Power;
        static int upgradesDifference;

        static int BaseCreaturePower => CharactersController.BasePower;
        static int BaseWeaponPower => WeaponsController.BasePower;

        public void Initialise()
        {
            instance = this;

            CharactersController.OnCharacterUpgradedEvent += OnCharacterSelectedOrUpgraded;
            CharactersController.OnCharacterSelectedEvent += OnCharacterSelectedOrUpgraded;

            WeaponsController.OnWeaponUpgraded += UpdateDifficulty;
            WeaponsController.OnNewWeaponSelected += UpdateDifficulty;

            if(showDevText)
            {
                var devTextObject = new GameObject("[BALANCE DEV TEXT]");
                devTextObject.transform.SetParent(UIController.MainCanvas.transform);
                devTextObject.transform.ResetLocal();

                var devRectTransform = devTextObject.AddComponent<RectTransform>();
                devRectTransform.anchorMin = new Vector2(0, 1);
                devRectTransform.anchorMax = new Vector2(0, 1);
                devRectTransform.pivot = new Vector2(0, 1);

                devRectTransform.sizeDelta = new Vector2(300, 145);
                devRectTransform.anchoredPosition = new Vector2(35, -325);

                devText = devTextObject.AddComponent<TextMeshProUGUI>();
                devText.fontSize = 28;
            }
        }

        public static void UpdateDifficulty()
        {
            if (LevelController.CurrentLevelData == null || instance.disableDifficulty)
            {
                CurrentDifficulty = Difficulty.Default;
                instance.UpdateDevText();
                PowerRequirement = 1;

                return;
            }

            PowerRequirement = WeaponsController.GetCeilingKeyPower(LevelController.CurrentLevelData.RequiredUpg) + CharactersController.GetCeilingUpgradePower(LevelController.CurrentLevelData.RequiredUpg);

            upgradesDifference = Mathf.RoundToInt((PowerRequirement - CurrentGeneralPower) / 6f);

            // up to 2 skips - easy
            if (upgradesDifference < 3)
            {
                CurrentDifficulty = Difficulty.Easy;
            }
            // up to 6 skips - normal
            else if (upgradesDifference < 7)
            {
                CurrentDifficulty = Difficulty.Normal;
            }
            // up to 12 skips - medium
            else if (upgradesDifference < 13)
            {
                CurrentDifficulty = Difficulty.Medium;
            }
            // more then 12 skips - hard
            else
            {
                CurrentDifficulty = Difficulty.Hard;
            }

            instance.UpdateDevText();
        }

        void OnCharacterSelectedOrUpgraded(CharacterType characterType, Character character)
        {
            UpdateDifficulty();
        }

        void OnWeaponUpgraded(BaseUpgrade upgrade)
        {
            var weaponUpg = upgrade as BaseWeaponUpgrade;

            if (weaponUpg != null)
            {
                UpdateDifficulty();
            }
        }

        public static DifficultySettings GetActiveDifficultySettings()
        {
            for (var i = 0; i < instance.difficultyPresets.Count; i++)
            {
                if (instance.difficultyPresets[i].Difficulty.Equals(CurrentDifficulty))
                {
                    return instance.difficultyPresets[i];
                }
            }

            Debug.LogError("[Balance Controller] Difficulty preset not found for: " + CurrentDifficulty);
            return instance.difficultyPresets[0];
        }

        void UpdateDevText()
        {
            if (LevelController.CurrentLevelData != null && showDevText)
            {
                devText.text = "lvl: " + (ActiveRoom.CurrentWorldIndex + 1) + "-" + (ActiveRoom.CurrentLevelIndex + 1)
                    + "\npwr: " + CurrentGeneralPower + "/" + PowerRequirement
                    + "\nupg: " + upgradesDifference
                    + "\ndif: " + CurrentDifficulty.ToString().ToLower();
            }
        }
    }
}