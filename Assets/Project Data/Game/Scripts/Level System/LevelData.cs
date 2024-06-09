using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class LevelData
    {
        [SerializeField] LevelType type;
        public LevelType Type => type;

        [SerializeField] RoomData[] rooms;
        public RoomData[] Rooms => rooms;

        [Space]
        [SerializeField] int xpAmount;
        public int XPAmount => xpAmount;

        [SerializeField] int requiredUpg;
        public int RequiredUpg => requiredUpg;

        [SerializeField] int enemiesLevel;
        public int EnemiesLevel => enemiesLevel;

        [SerializeField] bool hasCharacterSuggestion;
        public bool HasCharacterSuggestion => hasCharacterSuggestion;

        [SerializeField, Range(0.0f, 1.0f)] float healSpawnPercent = 0.5f;
        public float HealSpawnPercent => healSpawnPercent;

        [SerializeField] List<DropData> dropData = new();
        public List<DropData> DropData => dropData;

        [SerializeField] LevelSpecialBehaviour[] specialBehaviours;
        public LevelSpecialBehaviour[] SpecialBehaviours => specialBehaviours;

        WorldData world;
        public WorldData World => world;

        public void Initialise(WorldData world)
        {
            this.world = world;
        }

        #region Special Behaviours callbacks
        public void OnLevelInitialised()
        {
            foreach (var t in specialBehaviours)
                t.OnLevelInitialised();
        }

        public void OnLevelLoaded()
        {
            foreach (var t in specialBehaviours)
                t.OnLevelLoaded();
        }

        public void OnLevelUnloaded()
        {
            foreach (var t in specialBehaviours)
                t.OnLevelUnloaded();
        }

        public void OnLevelStarted()
        {
            foreach (var t in specialBehaviours)
                t.OnLevelStarted();
        }

        public void OnLevelFailed()
        {
            foreach (var t in specialBehaviours)
                t.OnLevelFailed();
        }

        public void OnLevelCompleted()
        {
            foreach (var t in specialBehaviours)
                t.OnLevelCompleted();
        }

        public void OnRoomEntered()
        {
            foreach (var t in specialBehaviours)
                t.OnRoomEntered();
        }

        public void OnRoomLeaved()
        {
            foreach (var t in specialBehaviours)
                t.OnRoomLeaved();
        }
        #endregion

        public int GetChestsAmount(bool includeRewarded = false)
            => rooms.Where(room => room.ChestEntities != null)
                .Sum(room => room.ChestEntities.Count(
                    chest => chest.IsInited && (includeRewarded || chest.ChestType != LevelChestType.Rewarded)));

        public int GetCoinsReward() 
            => (from data in dropData where data.dropType == DropableItemType.Currency && data.currencyType == CurrencyType.Coins select data.amount).FirstOrDefault();

        public List<WeaponType> GetCardsReward()
        {
            var result = new List<WeaponType>();

            foreach (var t in dropData)
            {
                if (t.dropType != DropableItemType.WeaponCard) continue;
                var isWeaponUnlocked = WeaponsController.IsWeaponUnlocked(t.cardType);
                if (isWeaponUnlocked) continue;
                for (var j = 0; j < t.amount; j++)
                    result.Add(t.cardType);
            }

            return result;
        }
    }
}