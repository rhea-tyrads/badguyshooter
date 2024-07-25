using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class CharactersController : MonoBehaviour
    {
        const CharacterType DEFAULT_CHARACTER_TYPE = CharacterType.Character01;
        static CharactersController _charactersController;
        [SerializeField] CharactersDatabase database;
        public static int BasePower { get; private set; }
        static Character _selectedCharacter;
        public static Character SelectedCharacter => _selectedCharacter;
        public static Character LastUnlockedCharacter => _charactersController.database.GetLastUnlocked();
        public static Character NextCharacterToUnlock => _charactersController.database.GetNextToUnlock();
        static CharacterGlobalSave _characterSave;
        public static event CharacterCallback OnCharacterSelectedEvent;
        public static event CharacterCallback OnCharacterUpgradedEvent;
        static readonly List<CharacterUpgrade> KEY_UPGRADES = new();

        public void Initialise()
        {
            _charactersController = this;
            database.Initialise();
            _characterSave = SaveController.GetSaveObject<CharacterGlobalSave>("characters");

            // Check if character from save is unlocked
            _selectedCharacter = database.Get(IsUnlocked(_characterSave.selectedCharacterType)
                ? _characterSave.selectedCharacterType
                : DEFAULT_CHARACTER_TYPE);

            foreach (var character in database.Characters)
            {
                foreach (var upgrade in character.Upgrades)
                {
                    if (upgrade.Stats.KeyUpgradeNumber == -1) continue;
                    KEY_UPGRADES.Add(upgrade);
                    if (upgrade.Stats.KeyUpgradeNumber == 0)
                        BasePower = upgrade.Stats.Power;
                }
            }

            KEY_UPGRADES.OrderBy(u => u.Stats.KeyUpgradeNumber);
        }

        public static bool IsUnlocked(CharacterType characterType)
        {
            var character = _charactersController.database.Get(characterType);
            return character != null && character.IsUnlocked();
        }

        public static void Select(CharacterType characterType)
        {
            if (_selectedCharacter.Type == characterType) return;

            var character = _charactersController.database.Get(characterType);
            if (character == null) return;

            _selectedCharacter = character;
            _characterSave.selectedCharacterType = characterType;
            var characterBehaviour = CharacterBehaviour.GetBehaviour();
            var characterStage = character.GetCurrentStage();
            var characterUpgrade = character.GetCurrentUpgrade();
            characterBehaviour.SetStats(characterUpgrade.Stats);
            characterBehaviour.SetGraphics(characterStage.Prefab, false, false);
            OnCharacterSelectedEvent?.Invoke(characterType, _selectedCharacter);
        }

        public static void OnCharacterUpgraded(Character character)
        {
            AudioController.Play(AudioController.Sounds.upgrade);
            OnCharacterUpgradedEvent?.Invoke(character.Type, character);
        }

        public static CharactersDatabase GetDatabase()
            => _charactersController.database;

        public static Character Get(CharacterType characterType) =>
            _charactersController.database.Get(characterType);

        public static int GetIndex(CharacterType characterType) =>
            System.Array.FindIndex(_charactersController.database.Characters, x => x.Type == characterType);

        public static int GetCeilingUpgradePower(int currentKeyUpgrade)
        {
            for (var i = KEY_UPGRADES.Count - 1; i >= 0; i--)
            {
                if (KEY_UPGRADES[i].Stats.KeyUpgradeNumber <= currentKeyUpgrade)
                    return KEY_UPGRADES[i].Stats.Power;
            }

            return KEY_UPGRADES[0].Stats.Power;
        }

        public delegate void CharacterCallback(CharacterType characterType, Character character);
    }
}