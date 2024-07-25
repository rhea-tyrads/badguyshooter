// ReSharper disable InconsistentNaming

using UnityEngine.Serialization;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class LevelSave : ISaveObject
    {
        [FormerlySerializedAs("WorldIndex")] public int World;
        [FormerlySerializedAs("LevelIndex")] public int Level;
        public int LastCompletedLevelCoinBalance;

        public LevelSave()
        {
            World = 0;
            Level = 0;
        }

        public void Flush()
        {

        }
    }
}