using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class Character
    {
        [SerializeField] string name;
        [SerializeField] CharacterType type;

        public CharacterType Type => type;
        public string Name => name;

        [SerializeField] int requiredLevel;
        public int RequiredLevel => requiredLevel;

        [SerializeField] Sprite lockedSprite;
        public Sprite LockedSprite => lockedSprite;

        [SerializeField] CharacterStageData[] stages;

        public CharacterStageData[] Stages => stages;
        [SerializeField] CharacterUpgrade[] upgrades;
        public CharacterUpgrade[] Upgrades => upgrades;

        CharacterSave _save;
        public CharacterSave Save => _save;


        public void Initialise()
        {
            _save = SaveController.GetSaveObject<CharacterSave>("Character" + type);

#if UNITY_EDITOR
            if (stages.IsNullOrEmpty())
                Debug.LogError(string.Format("[Character]: Character with type {0} has no stages!", type));
#endif
        }

        public CharacterStageData GetCurrentStage()
        {
            for (var i = _save.upgradeLevel; i >= 0; i--)
            {
                if (!upgrades[i].ChangeStage) continue;
                return stages[upgrades[i].StageIndex];
            }

            return stages[0];
        }

        public int GetCurrentStageIndex()
        {
            for (var i = _save.upgradeLevel; i >= 0; i--)
            {
                if (!upgrades[i].ChangeStage) continue;
                return i;
            }

            return 0;
        }

        public CharacterUpgrade GetCurrentUpgrade()
            => upgrades[_save.upgradeLevel];

        public CharacterUpgrade GetNextUpgrade()
            => upgrades.IsInRange(_save.upgradeLevel + 1) ? upgrades[_save.upgradeLevel + 1] : null;

        public int GetCurrentUpgradeIndex() => _save.upgradeLevel;

        public bool IsMaxUpgrade() => !upgrades.IsInRange(_save.upgradeLevel + 1);

        public void UpgradeCharacter()
        {
            if (!upgrades.IsInRange(_save.upgradeLevel + 1)) return;
            _save.upgradeLevel += 1;
            CharactersController.OnCharacterUpgraded(this);
        }

        public bool IsSelected() => CharactersController.SelectedCharacter.type == type;
        public bool onlyShop;
        public bool IsPurchased() => PlayerPrefs.HasKey(SaveKey);
        public string saveKey;
        public string SaveKey => "Skin_" + saveKey;
        public void Purchase() => PlayerPrefs.SetInt(SaveKey, 1);

        public bool IsUnlocked() => onlyShop
            ? IsPurchased()
            : ExperienceController.CurrentLevel >= requiredLevel;
    }
}