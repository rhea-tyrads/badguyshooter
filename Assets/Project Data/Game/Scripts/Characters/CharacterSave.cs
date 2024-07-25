using UnityEngine.Serialization;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterSave : ISaveObject
    {
        [FormerlySerializedAs("UpgradeLevel")] public int upgradeLevel = 0;

        public void Flush()
        {

        }
    }
}