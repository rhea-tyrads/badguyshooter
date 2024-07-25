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
        TextMeshProUGUI _devText;
        static BalanceController _instance;
        static Difficulty CurrentDifficulty { get; set; }
        public static int PowerRequirement { get; private set; }

        public static int CurrentGeneralPower
            => CharactersController.SelectedCharacter.GetCurrentUpgrade().Stats.Power +
               ((BaseWeaponUpgradeStage)WeaponsController.GetCurrentWeaponUpgrade().CurrentStage).Power;

        static int _upgradesDifference;

        static int BaseCreaturePower => CharactersController.BasePower;
        static int BaseWeaponPower => WeaponsController.BasePower;

        public void Initialise()
        {
            _instance = this;
            CharactersController.OnCharacterUpgradedEvent += OnCharacterSelectedOrUpgraded;
            CharactersController.OnCharacterSelectedEvent += OnCharacterSelectedOrUpgraded;
            WeaponsController.OnWeaponUpgraded += UpdateDifficulty;
            WeaponsController.OnNewWeaponSelected += UpdateDifficulty;

            if (showDevText)
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
                _devText = devTextObject.AddComponent<TextMeshProUGUI>();
                _devText.fontSize = 28;
            }
        }

        public static void UpdateDifficulty()
        {
            if (LevelController.CurrentLevelData == null || _instance.disableDifficulty)
            {
                CurrentDifficulty = Difficulty.Default;
                _instance.UpdateDevText();
                PowerRequirement = 1;

                return;
            }

            PowerRequirement = WeaponsController.GetCeilingKeyPower(LevelController.CurrentLevelData.RequiredUpg) +
                               CharactersController.GetCeilingUpgradePower(LevelController.CurrentLevelData
                                   .RequiredUpg);

            _upgradesDifference = Mathf.RoundToInt((PowerRequirement - CurrentGeneralPower) / 6f);

            CurrentDifficulty = _upgradesDifference switch
            {
                < 3 => Difficulty.Easy, // up to 2 skips - easy
                < 7 => Difficulty.Normal, // up to 6 skips - normal
                < 13 => Difficulty.Medium, // up to 12 skips - medium
                _ => Difficulty.Hard
            };

            _instance.UpdateDevText();
        }

        void OnCharacterSelectedOrUpgraded(CharacterType characterType, Character character)
        {
            UpdateDifficulty();
        }

        void OnWeaponUpgraded(BaseUpgrade upgrade)
        {
            var weaponUpg = upgrade as BaseWeaponUpgrade;

            if (weaponUpg != null)
                UpdateDifficulty();
        }

        public static DifficultySettings GetActiveDifficultySettings()
        {
            foreach (var settings in _instance.difficultyPresets)
            {
                if (!settings.Difficulty.Equals(CurrentDifficulty)) continue;
                return settings;
            }

            Debug.LogError("[Balance Controller] Difficulty preset not found for: " + CurrentDifficulty);
            return _instance.difficultyPresets[0];
        }

        void UpdateDevText()
        {
            if (LevelController.CurrentLevelData != null && showDevText)
            {
                _devText.text = "lvl: " + (ActiveRoom.World + 1) + "-" + (ActiveRoom.Level + 1)
                                + "\npwr: " + CurrentGeneralPower + "/" + PowerRequirement
                                + "\nupg: " + _upgradesDifference
                                + "\ndif: " + CurrentDifficulty.ToString().ToLower();
            }
        }
    }
}