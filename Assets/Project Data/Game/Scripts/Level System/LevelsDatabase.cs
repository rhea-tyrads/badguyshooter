using System.Collections.Generic;
using UnityEngine;


namespace Watermelon.LevelSystem
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Content/New Level/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField] WorldData[] worlds;
        public WorldData[] Worlds => worlds;

        public void Initialise()
        {
            foreach (var world in worlds)
                world.Initialise();
        }

        public WorldData GetWorld(int id) 
            => worlds.IsInRange(id)
                ? worlds[id] 
                : worlds[id % worlds.Length];

        LevelData GetRandomLevel()
        {
            LevelData tempLevel = null;

            do
            {
                var randomWorld = worlds.GetRandomItem();
                if (!randomWorld) continue;
                var randomLevel = randomWorld.Levels.GetRandomItem();
                if (randomLevel != null)
                    tempLevel = randomLevel;
            }
            while (tempLevel == null);

            return tempLevel;
        }

        public LevelData GetLevel(int worldIndex, int levelIndex)
        {
            var world = GetWorld(worldIndex);
            if (!world) return GetRandomLevel();
            return world.Levels.IsInRange(levelIndex) ? world.Levels[levelIndex] : GetRandomLevel();
        }

        public bool IsNextLevel(int worldIndex, int levelIndex)
        {
            var world = GetWorld(worldIndex);
            return world && world.Levels.IsInRange(levelIndex + 1);
        }
        public bool IsNextWorld(int id ) => worlds.IsInRange(id);

        public int GetRandomWorld(int id)
        {
            for (var i = 0; i < 100; i++)
            {
                var r = Random.Range(2, worlds.Length);
                if (r != id) return r;
            }

            return 5;
        }
        public int TotalWorlds => worlds.Length;
    }
}
