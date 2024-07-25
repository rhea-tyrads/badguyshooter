using UnityEngine.Serialization;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterGlobalSave : ISaveObject
    {
        [FormerlySerializedAs("SelectedCharacterType")] public CharacterType selectedCharacterType = CharacterType.Character01;

        public void Flush()
        {

        }
    }
}