using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class CharactersController : MonoBehaviour
    {
        const CharacterType DEFAULT_CHARACTER_TYPE = CharacterType.Character_01;

        static CharactersController charactersController;

        [SerializeField] CharactersDatabase database;

        public static int BasePower { get; private set; }

        static Character selectedCharacter;
        public static Character SelectedCharacter => selectedCharacter;

        public static Character LastUnlockedCharacter => charactersController.database.GetLastUnlockedCharacter();
        public static Character NextCharacterToUnlock => charactersController.database.GetNextCharacterToUnlock();

        static CharacterGlobalSave characterSave;

        public static event CharacterCallback OnCharacterSelectedEvent;
        public static event CharacterCallback OnCharacterUpgradedEvent;

        static List<CharacterUpgrade> keyUpgrades = new();

        public void Initialise()
        {
            charactersController = this;
            database.Initialise();
            characterSave = SaveController.GetSaveObject<CharacterGlobalSave>("characters");

            // Check if character from save is unlocked
            selectedCharacter = database.GetCharacter(IsCharacterUnlocked(characterSave.SelectedCharacterType)
                ? characterSave.SelectedCharacterType
                : DEFAULT_CHARACTER_TYPE);

            foreach (var character in database.Characters)
            {
                foreach (var upgrade in character.Upgrades)
                {
                    if (upgrade.Stats.KeyUpgradeNumber == -1) continue;
                    keyUpgrades.Add(upgrade);
                    if (upgrade.Stats.KeyUpgradeNumber == 0)
                        BasePower = upgrade.Stats.Power;
                }
            }

            keyUpgrades.OrderBy(u => u.Stats.KeyUpgradeNumber);
        }

        public static bool IsCharacterUnlocked(CharacterType characterType)
        {
            var character = charactersController.database.GetCharacter(characterType);
            return character != null && character.IsUnlocked();
        }

        public static void SelectCharacter(CharacterType characterType)
        {
            if (selectedCharacter.Type == characterType) return;

            var character = charactersController.database.GetCharacter(characterType);
            if (character == null) return;

            selectedCharacter = character;
            characterSave.SelectedCharacterType = characterType;
            var characterBehaviour = CharacterBehaviour.GetBehaviour();
            var characterStage = character.GetCurrentStage();
            var characterUpgrade = character.GetCurrentUpgrade();
            characterBehaviour.SetStats(characterUpgrade.Stats);
            characterBehaviour.SetGraphics(characterStage.Prefab, false, false);
            OnCharacterSelectedEvent?.Invoke(characterType, selectedCharacter);
        }

        public static void OnCharacterUpgraded(Character character)
        {
            AudioController.PlaySound(AudioController.Sounds.upgrade);
            OnCharacterUpgradedEvent?.Invoke(character.Type, character);
        }

        public static CharactersDatabase GetDatabase()
            => charactersController.database;

        public static Character GetCharacter(CharacterType characterType) =>
            charactersController.database.GetCharacter(characterType);

        public static int GetCharacterIndex(CharacterType characterType) =>
            System.Array.FindIndex(charactersController.database.Characters, x => x.Type == characterType);

        public static int GetCeilingUpgradePower(int currentKeyUpgrade)
        {
            for (var i = keyUpgrades.Count - 1; i >= 0; i--)
            {
                if (keyUpgrades[i].Stats.KeyUpgradeNumber <= currentKeyUpgrade)
                    return keyUpgrades[i].Stats.Power;
            }

            return keyUpgrades[0].Stats.Power;
        }

        public delegate void CharacterCallback(CharacterType characterType, Character character);
    }
}