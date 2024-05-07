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

        static List<CharacterUpgrade> keyUpgrades = new List<CharacterUpgrade>();

        public void Initialise()
        {
            charactersController = this;

            // Initialise characters database
            database.Initialise();

            // Get global save
            characterSave = SaveController.GetSaveObject<CharacterGlobalSave>("characters");

            // Check if character from save is unlocked
            if (IsCharacterUnlocked(characterSave.SelectedCharacterType))
            {
                // Load selected character from save
                selectedCharacter = database.GetCharacter(characterSave.SelectedCharacterType);
            }
            else
            {
                // Select default character
                selectedCharacter = database.GetCharacter(DEFAULT_CHARACTER_TYPE);
            }

            for (var i = 0; i < database.Characters.Length; i++)
            {
                var character = database.Characters[i];

                for (var j = 0; j < character.Upgrades.Length; j++)
                {
                    if (character.Upgrades[j].Stats.KeyUpgradeNumber != -1)
                    {
                        keyUpgrades.Add(character.Upgrades[j]);

                        if (character.Upgrades[j].Stats.KeyUpgradeNumber == 0)
                        {
                            BasePower = character.Upgrades[j].Stats.Power;
                        }
                    }
                }
            }

            keyUpgrades.OrderBy(u => u.Stats.KeyUpgradeNumber);
        }

        public static bool IsCharacterUnlocked(CharacterType characterType)
        {
            var character = charactersController.database.GetCharacter(characterType);
            if (character != null)
                return character.IsUnlocked();

            return false;
        }

        public static void SelectCharacter(CharacterType characterType)
        {
            if (selectedCharacter.Type == characterType)
                return;

            var character = charactersController.database.GetCharacter(characterType);
            if (character != null)
            {
                selectedCharacter = character;

                characterSave.SelectedCharacterType = characterType;

                var characterBehaviour = CharacterBehaviour.GetBehaviour();

                var characterStage = character.GetCurrentStage();
                var characterUpgrade = character.GetCurrentUpgrade();

                characterBehaviour.SetStats(characterUpgrade.Stats);
                characterBehaviour.SetGraphics(characterStage.Prefab, false, false);

                // Invoke select character callback
                OnCharacterSelectedEvent?.Invoke(characterType, selectedCharacter);
            }
        }

        public static void OnCharacterUpgraded(Character character)
        {
            AudioController.PlaySound(AudioController.Sounds.upgrade);

            OnCharacterUpgradedEvent?.Invoke(character.Type, character);
        }

        public static CharactersDatabase GetDatabase()
        {
            return charactersController.database;
        }

        public static Character GetCharacter(CharacterType characterType)
        {
            return charactersController.database.GetCharacter(characterType);
        }

        public static int GetCharacterIndex(CharacterType characterType)
        {
            return System.Array.FindIndex(charactersController.database.Characters, x => x.Type == characterType);
        }

        public static int GetCeilingUpgradePower(int currentKeyUpgrade)
        {
            for (var i = keyUpgrades.Count - 1; i >= 0; i--)
            {
                if (keyUpgrades[i].Stats.KeyUpgradeNumber <= currentKeyUpgrade)
                {
                    return keyUpgrades[i].Stats.Power;
                }
            }

            return keyUpgrades[0].Stats.Power;
        }

        public delegate void CharacterCallback(CharacterType characterType, Character character);
    }
}