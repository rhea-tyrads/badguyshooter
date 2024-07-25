using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.SquadShooter;
using static UnityEngine.Object;

namespace Watermelon.LevelSystem
{
    public static class ActiveRoom
    {
        static GameObject levelObject;
 
        static RoomData roomData;
        public static RoomData RoomData => roomData;

        static LevelData levelData;
        public static LevelData LevelData => levelData;

        static List<GameObject> activeObjects;

        static List<BaseEnemyBehavior> enemies;
        public static List<BaseEnemyBehavior> Enemies => enemies;

        static List<AbstractChestBehavior> chests;
        public static List<AbstractChestBehavior> Chests => chests;

        static int _level;
        public static int Level => _level;

        static int _world;
        public static int World => _world;

        static ExitPointBehaviour exitPointBehaviour;
        public static ExitPointBehaviour ExitPointBehaviour => exitPointBehaviour;

        static List<GameObject> customObjects;
        public static List<GameObject> CustomObjects => customObjects;

        public static void Initialise(GameObject levelObject)
        {
            ActiveRoom.levelObject = levelObject;

            activeObjects = new List<GameObject>();
            enemies = new List<BaseEnemyBehavior>();
            chests = new List<AbstractChestBehavior>();
            customObjects = new List<GameObject>();
        }

        public static void SetLevelData(int currentWorldIndex, int currentLevelIndex)
        {
            _world = currentWorldIndex;
            _level = currentLevelIndex;
        }

        public static void SetLevelData(LevelData data)
        {
            levelData = data;
        }

        public static void SetRoomData(RoomData data)
        {
            roomData = data;
        }

        public static void Unload()
        {
            // Unload created obstacles
            foreach (var a in activeObjects)
            {
                a.transform.SetParent(null);
                a.SetActive(false);
            }

            activeObjects.Clear();

            foreach (var enemy in enemies)
            {
                enemy.Unload();
                Destroy(enemy.gameObject);
            }

            enemies.Clear();

            if (exitPointBehaviour)
            {
                exitPointBehaviour.Unload();
                Destroy(exitPointBehaviour.gameObject);
                exitPointBehaviour = null;
            }

            UnloadCustomObjects();
        }

        #region Environment/Obstacles

        public static void SpawnItem(LevelItem item, ItemEntityData itemEntityData)
        {
            var itemObject = item.Pool.Get(false);
            itemObject.transform.SetParent(levelObject.transform);
            itemObject.transform.SetPositionAndRotation(itemEntityData.Position, itemEntityData.Rotation);
            itemObject.transform.localScale = itemEntityData.Scale;
            itemObject.SetActive(true);

            activeObjects.Add(itemObject);
        }

        public static void SpawnExitPoint(GameObject exitPointPrefab, Vector3 position)
        {
            exitPointBehaviour = Instantiate(exitPointPrefab, position, Quaternion.identity, levelObject.transform)
                .GetComponent<ExitPointBehaviour>();
            exitPointBehaviour.Initialise();
        }

        public static void SpawnChest(ChestEntityData chestEntityData, ChestData chestData)
        {
            var chestObject = chestData.Pool.Get(false);
            chestObject.transform.SetParent(levelObject.transform);
            chestObject.transform.SetPositionAndRotation(chestEntityData.Position, chestEntityData.Rotation);
            chestObject.transform.localScale = chestEntityData.Scale;
            chestObject.SetActive(true);

            chests.Add(chestObject.GetComponent<AbstractChestBehavior>());

            activeObjects.Add(chestObject);
        }

        #endregion

        #region Enemies

        public static BaseEnemyBehavior SpawnEnemy(EnemyData enemyData, EnemyEntityData enemyEntityData, bool isActive)
        {
            var enemy = Instantiate(enemyData.Prefab, enemyEntityData.Position, enemyEntityData.Rotation, levelObject.transform).GetComponent<BaseEnemyBehavior>();
            enemy.transform.localScale = enemyEntityData.Scale;
            enemy.SetEnemyData(enemyData, enemyEntityData.IsElite);
            enemy.SetPatrollingPoints(enemyEntityData.PathPoints);

            // Place enemy on the middle of the path if there are two or more waypoints
            if (enemyEntityData.PathPoints.Length > 1)
                enemy.transform.position = enemyEntityData.PathPoints[0] +
                                           (enemyEntityData.PathPoints[1] - enemyEntityData.PathPoints[0]) * 0.5f;

            if (isActive)
                enemy.Initialise();

            enemies.Add(enemy);

            return enemy;
        }

        public static void ActivateEnemies()
        {
            for (var i = 0; i < enemies.Count; i++)
            {
                enemies[i].Initialise();
            }
        }

        public static void ClearEnemies()
        {
            foreach (var enemy in enemies)
            {
                enemy.Unload();

                Destroy(enemy.gameObject);
            }

            enemies.Clear();
        }

        public static BaseEnemyBehavior GetEnemyForSpecialReward()
        {
            var result = enemies.Find(e => e.Tier == EnemyTier.Boss);
            if (result) return result;

            result = enemies.Find(e => e.Tier == EnemyTier.Elite);
            if (result) return result;

            result = enemies[0];

            for (var i = 1; i < enemies.Count; i++)
            {
                if (enemies[i].transform.position.z > result.transform.position.z)
                    result = enemies[i];
            }

            return result;
        }

        public static void InitialiseDrop(List<DropData> enemyDrop, List<DropData> chestDrop)
        {
            foreach (var enemy in enemies)
            {
                enemy.ResetDrop();
            }

            foreach (var drop in enemyDrop)
            {
                if (drop.dropType == DropableItemType.Currency && drop.currencyType == CurrencyType.Coins)
                {
                    var coins = LevelController.SplitIntEqually(drop.amount, enemies.Count);

                    for (var j = 0; j < enemies.Count; j++)
                    {
                        enemies[j].AddDrop(new DropData()
                        {
                            dropType = DropableItemType.Currency, currencyType = CurrencyType.Coins, amount = coins[j]
                        });
                    }
                }
                else
                {
                    GetEnemyForSpecialReward().AddDrop(drop);
                }
            }

            foreach (var chest in chests)
            {
                chest.Init(chestDrop);
            }
        }

        public static List<BaseEnemyBehavior> GetAliveEnemies() => enemies.Where(enemy => !enemy.IsDead).ToList();

        public static bool AreAllEnemiesDead()
            => enemies.All(t => t.IsDead);

        #endregion

        #region Custom Objects

        public static void SpawnCustomObject(CustomObjectData objectData)
        {
            var customObject = Instantiate(objectData.PrefabRef);
            customObject.transform.SetParent(levelObject.transform);
            customObject.transform.SetPositionAndRotation(objectData.Position, objectData.Rotation);
            customObject.transform.localScale = objectData.Scale;
            customObject.SetActive(true);

            customObjects.Add(customObject);
        }

        public static void UnloadCustomObjects()
        {
            if (customObjects.IsNullOrEmpty())
                return;

            foreach (var obj in customObjects)
            {
                Destroy(obj);
            }

            customObjects.Clear();
        }

        #endregion
    }
}