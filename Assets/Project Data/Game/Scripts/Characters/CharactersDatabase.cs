using System.Linq;
using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Character Database", menuName = "Content/Characters/Character Database")]
    public class CharactersDatabase : ScriptableObject
    {
        [SerializeField] Character[] characters;
        public Character[] Characters => characters;

        public void Initialise()
        {
            characters.OrderBy(c => c.RequiredLevel);
            foreach (var character in characters)
                character.Initialise();
        }

        public Character Get(CharacterType characterType) 
            => characters.FirstOrDefault(character => character.Type == characterType);

        public Character GetLastUnlocked()
        {
            for (var i = 0; i < characters.Length; i++)
            {
                if (characters[i].RequiredLevel <= ExperienceController.CurrentLevel) continue;
                return characters[Mathf.Clamp(i - 1, 0, characters.Length - 1)];
            }
            return null;
        }

        public Character GetNextToUnlock() 
            => characters.FirstOrDefault(character => character.RequiredLevel > ExperienceController.CurrentLevel);
    }
}