using System.Collections.Generic;
using MobileTools.SDK;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public static class LevelController
    {
        static LevelsDatabase levelsDatabase;
        public static LevelsDatabase LevelsDatabase => levelsDatabase;
        static GameSettings levelSettings;
        public static GameSettings LevelSettings => levelSettings;
        static GameObject levelGameObject;
        public static GameObject LevelGameObject => levelGameObject;
        static GameObject backWallCollider;
        static bool isLevelLoaded;
        static LevelData loadedLevel;
        static bool isRoomLoaded;
        static RoomData loadedRoom;
        static LevelSave levelSave;
        static LevelData currentLevelData;
        public static LevelData CurrentLevelData => currentLevelData;
        public static int currentRoomIndex;
        public static CharacterBehaviour characterBehaviour; // Player
        static GameObject playerObject;
        static WorldData activeWorldData;
        static UIComplete uiComplete;
        static UIMainMenu uiMainMenu;
        public static UIGame uiGame;
        static UICharacterSuggestion uiCharacterSuggestion;

        static bool manualExitActivation;
        static int lastLevelMoneyCollected;
        static bool isGameplayActive;
        public static bool IsGameplayActive => isGameplayActive;
        static bool needCharacterSugession;
        public static bool NeedCharacterSugession => needCharacterSugession;
        static List<List<DropData>> roomRewards;
        static List<List<DropData>> roomChestRewards;
        public static event SimpleCallback OnPlayerExitLevelEvent;
        public static event SimpleCallback OnPlayerDiedEvent;

        // Pedestal
        public static PedestalBehavior PedestalBehavior { get; private set; }

        public static void Initialise()
        {
            levelSettings = GameController.Settings;
            levelsDatabase = levelSettings.LevelsDatabase;
            levelsDatabase.Initialise();
            levelSave = SaveController.GetSaveObject<LevelSave>("level");
            levelGameObject = new GameObject("[LEVEL]");
            levelGameObject.transform.ResetGlobal();
            backWallCollider = MonoBehaviour.Instantiate(levelSettings.BackWallCollider, Vector3.forward * -1000f,
                Quaternion.identity, levelGameObject.transform);

            // UI
            uiComplete = UIController.GetPage<UIComplete>();
            uiMainMenu = UIController.GetPage<UIMainMenu>();
            uiGame = UIController.GetPage<UIGame>();
            uiCharacterSuggestion = UIController.GetPage<UICharacterSuggestion>();
            NavMeshController.Initialise(levelGameObject, levelSettings.NavMeshData);
            ActiveRoom.Initialise(levelGameObject);

            // Store current level
            currentLevelData = levelsDatabase.GetLevel(levelSave.WorldIndex, levelSave.LevelIndex);
        }

        public static void SpawnPlayer()
        {
            var character = CharactersController.SelectedCharacter;
            var characterStage = character.GetCurrentStage();
            var characterUpgrade = character.GetCurrentUpgrade();

            // Spawn player
            playerObject = Object.Instantiate(levelSettings.PlayerPrefab);
            playerObject.name = "[CHARACTER]";
            
            CameraController.SetMainTarget(playerObject.transform);
            characterBehaviour = playerObject.GetComponent<CharacterBehaviour>();
            characterBehaviour.SetStats(characterUpgrade.Stats);
            characterBehaviour.Initialise();
            characterBehaviour.SetGraphics(characterStage.Prefab, false, false);
            characterBehaviour.SetGun(WeaponsController.GetCurrentWeapon(), false);
        }

        public static void LoadCurrentLevel()
        {
            Debug.LogWarning(levelSave.WorldIndex);
            LoadLevel(levelSave.WorldIndex, levelSave.LevelIndex);
        }

        public static void LoadLevel(int worldIndex, int levelIndex)
        {
            if (isLevelLoaded) return;
            isLevelLoaded = true;

            var levelData = levelsDatabase.GetLevel(worldIndex, levelIndex);
            ActiveRoom.SetLevelData(levelData);
            currentLevelData = levelData;
            currentLevelData.OnLevelInitialised();
            ActiveRoom.SetLevelData(worldIndex, levelIndex);

            var world = levelData.World;
            ActivateWorld(world);

            BalanceController.UpdateDifficulty();
            lastLevelMoneyCollected = 0;
            Control.DisableMovementControl();
            uiGame.UpdateCoinsText(CurrenciesController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);
            uiGame.InitRoomsUI(levelData.Rooms);
            uiMainMenu.LevelProgressionPanel.LoadPanel();
            uiMainMenu.UpdateLevelText();
            currentRoomIndex = 0;
            DistributeRewardBetweenRooms();

            // Load first room
            LoadRoom(currentRoomIndex);

            if (levelSave.LevelIndex != 0 || levelSave.WorldIndex > 0)
            {
                characterBehaviour.DisableAgent();
                LoadPedestal();
            }
        }

        static void DistributeRewardBetweenRooms()
        {
            var roomsAmount = currentLevelData.Rooms.Length;
            var chestsAmount = currentLevelData.GetChestsAmount();
            var moneyPerRoomOrChest = new List<int>();

            // find coins reward amount
            var coinsReward = currentLevelData.DropData
                .Find(d => d.dropType == DropableItemType.Currency && d.currencyType == CurrencyType.Coins);

            if (coinsReward != null)
            {
                // split coins reward equally between all rooms and chests 
                moneyPerRoomOrChest = SplitIntEqually(coinsReward.amount, roomsAmount + chestsAmount);
            }

            roomRewards = new List<List<DropData>>();
            roomChestRewards = new List<List<DropData>>();

            // creating reward for each room individually
            for (var i = 0; i < roomsAmount; i++)
            {
                roomRewards.Add(new List<DropData>());

                // if threre is money reward - assign this room's part
                if (moneyPerRoomOrChest.Count > 0 && moneyPerRoomOrChest[i] > 0)
                {
                    roomRewards[i].Add(new DropData()
                    {
                        dropType = DropableItemType.Currency, currencyType = CurrencyType.Coins,
                        amount = moneyPerRoomOrChest[i]
                    });
                }

                // if room is last - give special reward
                if (i != roomsAmount - 1) continue;

                foreach (var data in currentLevelData.DropData)
                {
                    // if it's not coins - then it's a special reward
                    if (data.dropType == DropableItemType.Currency && data.currencyType == CurrencyType.Coins) continue;
                    var skipThisReward = data.dropType == DropableItemType.WeaponCard &&
                                         WeaponsController.IsWeaponUnlocked(data.cardType);
                    // skip weapon card if weapon is already unlocked
                    if (!skipThisReward)
                        roomRewards[i].Add(data);
                }
            }

            var chestsSpawned = 0;

            for (var i = 0; i < roomsAmount; i++)
            {
                var room = currentLevelData.Rooms[i];

                if (room.ChestEntities != null && room.ChestEntities.Length > 0)
                {
                    foreach (var chest in room.ChestEntities)
                    {
                        if (chest.IsInited)
                        {
                            if (chest.ChestType == LevelChestType.Standart)
                            {
                                roomChestRewards.Add(new List<DropData>()
                                {
                                    new()
                                    {
                                        dropType = DropableItemType.Currency, currencyType = CurrencyType.Coins,
                                        amount = moneyPerRoomOrChest[roomsAmount + chestsSpawned]
                                    }
                                });

                                chestsSpawned++;
                            }
                            else
                            {
                                roomChestRewards.Add(new List<DropData>()
                                {
                                    new()
                                    {
                                        dropType = DropableItemType.Currency, currencyType = CurrencyType.Coins,
                                        amount = coinsReward.amount
                                    }
                                });
                            }
                        }
                        else
                        {
                            roomChestRewards.Add(new List<DropData>());
                        }
                    }
                }
                else
                {
                    roomChestRewards.Add(new List<DropData>());
                }
            }
        }

        static bool DoesNextRoomExist()
            => isLevelLoaded && currentLevelData.Rooms.IsInRange(currentRoomIndex + 1);

        static void LoadRoom(int index)
        {
            var roomData = currentLevelData.Rooms[index];
            ActiveRoom.SetRoomData(roomData);
            backWallCollider.transform.localPosition = roomData.SpawnPoint;
            manualExitActivation = false;

            // Reposition player
            characterBehaviour.SetPosition(roomData.SpawnPoint);
            characterBehaviour.Reload(index == 0);
            NavMeshController.InvokeOrSubscribe(characterBehaviour);

            var items = roomData.ItemEntities;
            foreach (var data in items)
            {
                var itemData = activeWorldData.GetLevelItem(data.Hash);
                if (itemData == null)
                {
                    Debug.Log("[Level Controller] Not found item with hash: " + data.Hash + " for the world: " +
                              activeWorldData.name);
                    continue;
                }

                ActiveRoom.SpawnItem(itemData, data);
            }


            var enemies = roomData.EnemyEntities;
            foreach (var enemy in enemies)
                ActiveRoom.SpawnEnemy(EnemyController.Database.GetEnemyData(enemy.EnemyType), enemy, false);

            ActiveRoom.SpawnExitPoint(levelSettings.ExitPointPrefab, roomData.ExitPoint);

            if (roomData.ChestEntities != null)
            {
                foreach (var chest in roomData.ChestEntities)
                {
                    if (!chest.IsInited) continue;
                    ActiveRoom.SpawnChest(chest, LevelSettings.GetChestData(chest.ChestType));
                }
            }

            var roomCustomObjects = roomData.RoomCustomObjects;
            foreach (var custom in roomCustomObjects)
                ActiveRoom.SpawnCustomObject(custom);

            var worldCustomObjects = levelsDatabase.GetWorld(levelSave.WorldIndex).WorldCustomObjects;
            foreach (var custom in worldCustomObjects)
                ActiveRoom.SpawnCustomObject(custom);

            ActiveRoom.InitialiseDrop(roomRewards[index], roomChestRewards[index]);
            currentLevelData.OnLevelLoaded();
            currentLevelData.OnRoomEntered();
            loadedLevel = currentLevelData;

            NavMeshController.RecalculateNavMesh(null);
            GameLoading.MarkAsReadyToHide();
        }

        public static void ReviveCharacter()
        {
            characterBehaviour.SetPosition(CurrentLevelData.Rooms[currentRoomIndex].SpawnPoint);
            isGameplayActive = true;
            characterBehaviour.Reload();
            characterBehaviour.Activate(false);
            characterBehaviour.SetPosition(CurrentLevelData.Rooms[currentRoomIndex].SpawnPoint);
            characterBehaviour.ResetDetector();
            Control.EnableMovementControl();
        }

        static void LoadPedestal()
        {
            PedestalBehavior = Object.Instantiate(activeWorldData.PedestalPrefab).GetComponent<PedestalBehavior>();
            PedestalBehavior.transform.position = LevelSettings.PedestalPosition;
            PedestalBehavior.PlaceCharacter();
        }

        public static void OnLevelFailed()
        {
            currentLevelData.OnLevelFailed();
        }

        public static void ReloadRoom()
        {
            if (!isLevelLoaded) return;

            NavMeshController.ClearAgents();
            characterBehaviour.Disable();
            characterBehaviour.Reload();
            ActiveRoom.ClearEnemies();
            currentRoomIndex = 0;
            uiGame.UpdateReachedRoomUI(currentRoomIndex);

            var roomData = currentLevelData.Rooms[currentRoomIndex];
            var enemies = roomData.EnemyEntities;
            foreach (var enemy in enemies)
                ActiveRoom.SpawnEnemy(EnemyController.Database.GetEnemyData(enemy.EnemyType), enemy, false);

            ActiveRoom.InitialiseDrop(roomRewards[currentRoomIndex], roomChestRewards[currentRoomIndex]);
            currentLevelData.OnRoomEntered();
            characterBehaviour.gameObject.SetActive(true);
            characterBehaviour.SetPosition(roomData.SpawnPoint);
            NavMeshController.InvokeOrSubscribe(characterBehaviour);
        }

        public static void UnloadLevel()
        {
            if (!isLevelLoaded) return;

            NavMeshController.Reset();
            characterBehaviour.Disable();
            loadedLevel.OnLevelUnloaded();
            ActiveRoom.Unload();
            isLevelLoaded = false;
            loadedLevel = null;
        }

        static void ActivateWorld(WorldData data)
        {
            if (activeWorldData && activeWorldData.Equals(data)) return;

            if (activeWorldData)
                activeWorldData.UnloadWorld();

            activeWorldData = data;
            activeWorldData.LoadWorld();
        }

        public static void StartGameplay()
        {
            if (ActiveRoom.CurrentWorldIndex == 0 && ActiveRoom.CurrentLevelIndex == 0)
            {
                uiGame.EnableCanvas();
                EnemyController.OnLevelWillBeStarted();

                if (NavMeshController.IsNavMeshCalculated)
                {
                    NavMeshController.ForceActivation();
                    StartGameplayOnceNavmeshIsReady();
                }
                else
                {
                    NavMeshController.RecalculateNavMesh(StartGameplayOnceNavmeshIsReady);
                }
            }
            else
            {
                BonusController.Show();
                uiGame.DisableCanvas();
                GameController.OnGameStarted();
            }
        }

        public static void StartGame()
        {
            uiGame.EnableCanvas();

            EnemyController.OnLevelWillBeStarted();

            if (NavMeshController.IsNavMeshCalculated)
            {
                NavMeshController.ForceActivation();

                StartGameplayOnceNavmeshIsReady();
            }
            else
            {
                NavMeshController.RecalculateNavMesh(StartGameplayOnceNavmeshIsReady);
            }
        }

        static void StartGameplayOnceNavmeshIsReady()
        {
            GameController.OnGameStarted();
            ActiveRoom.ActivateEnemies();
            characterBehaviour.Activate();
            Control.EnableMovementControl();
            currentLevelData.OnLevelStarted();
        }

        public static void EnableManualExitActivation()
        {
            manualExitActivation = true;
        }

        public static void ActivateExit()
        {
            if (!ActiveRoom.AreAllEnemiesDead()) return;
            ActiveRoom.ExitPointBehaviour.OnExitActivated();
            Vibration.Vibrate(VibrationIntensity.Medium);
        }

        public static void Unload()
        {
            characterBehaviour.MoveForwardAndDisable(0.3f);
            Control.DisableMovementControl();
            currentLevelData.OnRoomLeaved();
            ActiveRoom.Unload();
            NavMeshController.Reset();
        }
        
        public static void OnPlayerExitLevel()
        {
            OnPlayerExitLevelEvent?.Invoke();
            characterBehaviour.MoveForwardAndDisable(0.3f);
            Control.DisableMovementControl();
            currentRoomIndex++;
            currentLevelData.OnRoomLeaved();

            if (currentLevelData.Rooms.IsInRange(currentRoomIndex))
            {
                Overlay.Show(0.3f, () =>
                {
                    Debug.LogWarning(("ROOM CLEARED: " + currentRoomIndex));
                    uiGame.UpdateReachedRoomUI(currentRoomIndex);
                    ActiveRoom.Unload();
                    NavMeshController.Reset();
                    LoadRoom(currentRoomIndex);
                    NavMeshController.InvokeOrSubscribe(new NavMeshCallback(delegate
                    {
                        Control.EnableMovementControl();

                        characterBehaviour.Activate();
                        characterBehaviour.ActivateAgent();
                        ActiveRoom.ActivateEnemies();
                    }));

                    Overlay.Hide(0.3f, null);
                });
            }
            else
            {
                SDKEvents.Instance.LevelComplete(ActiveRoom.CurrentWorldIndex + 1, ActiveRoom.CurrentLevelIndex);
                uiGame.UpdateReachedRoomUI(currentRoomIndex);
                OnLevelCompleted();
            }
        }

        public static void OnEnemyKilled(BaseEnemyBehavior enemyBehavior)
        {
            if (manualExitActivation) return;
            ActivateExit();
        }

        public static void OnCoinPicked(int amount)
        {
            lastLevelMoneyCollected += amount;
            uiGame.UpdateCoinsText(CurrenciesController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);
        }

        public static void OnRewardedCoinPicked(int amount)
        {
            CurrenciesController.Add(CurrencyType.Coins, amount);
            uiGame.UpdateCoinsText(CurrenciesController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);
        }

        public static void OnGameStarted(bool immediately = false)
        {
            CustomMusicController.ToggleMusic(AudioController.Music.gameMusic, 0.3f, 0.3f);
            isGameplayActive = true;
            CameraController.SetCameraShiftState(true);
            CameraController.EnableCamera(CameraType.Main);

            lastLevelMoneyCollected = 0;
            uiGame.UpdateCoinsText(CurrenciesController.Get(CurrencyType.Coins) + lastLevelMoneyCollected);

            characterBehaviour.SetPosition(CurrentLevelData.Rooms[currentRoomIndex].SpawnPoint);
            Tween.NextFrame(() =>
            {
                characterBehaviour.Activate();
                characterBehaviour.ActivateMovement();
                characterBehaviour.ActivateAgent();
            });

            if (PedestalBehavior)
                Object.Destroy(PedestalBehavior.gameObject);

            if (immediately)
            {
                uiMainMenu.DisableCanvas();
                UIController.ShowPage<UIGame>();
                Control.EnableMovementControl();
                StartGameplay();
                UIGamepadButton.DisableAllTags();
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Game);
            }
            else
            {
                UIController.HidePage<UIMainMenu>(() =>
                {
                    UIController.ShowPage<UIGame>();
                    Control.EnableMovementControl();
                    StartGameplay();
                });
            }
        }

        public static void OnLevelCompleted()
        {
            isGameplayActive = false;
            CurrenciesController.Add(CurrencyType.Coins, CurrentLevelData.GetCoinsReward());
            WeaponsController.AddCards(CurrentLevelData.GetCardsReward());
            uiComplete.UpdateExperienceLabel(currentLevelData.XPAmount);
            InitialiseCharacterSuggestion();
            IncreaseLevelInSave();
            SaveController.MarkAsSaveIsRequired();
            GameController.LevelComplete();
            currentLevelData.OnLevelCompleted();
        }

        static void InitialiseCharacterSuggestion()
        {
            if (!currentLevelData.HasCharacterSuggestion)
            {
                needCharacterSugession = false;
                return;
            }

            var lastUnlockedCharacter = CharactersController.LastUnlockedCharacter;
            var nextCharacterToUnlock = CharactersController.NextCharacterToUnlock;

            if (lastUnlockedCharacter == null || nextCharacterToUnlock == null)
            {
                needCharacterSugession = false;
                return;
            }

            var lastXp = ExperienceController.XpRequiredForLevel(lastUnlockedCharacter.RequiredLevel);
            var nextXp = ExperienceController.XpRequiredForLevel(nextCharacterToUnlock.RequiredLevel);
            var lastProgress = (float) (ExperienceController.ExperiencePoints - lastXp) /
                               (nextXp - lastXp);
            var currentProgress =
                (float) (ExperienceController.ExperiencePoints + currentLevelData.XPAmount - lastXp) /
                (nextXp - lastXp);

            uiCharacterSuggestion.SetData(lastProgress, currentProgress, nextCharacterToUnlock);
            needCharacterSugession = true;
        }

        static void IncreaseLevelInSave()
        {
            if (levelsDatabase.DoesNextLevelExist(levelSave.WorldIndex, levelSave.LevelIndex))
                levelSave.LevelIndex++;
            else
            {
                levelSave.WorldIndex++;
                levelSave.LevelIndex = 0;
            }
        }

        public static void OnPlayerDied()
        {
            if (!IsGameplayActive) return;

            isGameplayActive = false;
            OnPlayerDiedEvent?.Invoke();
            Control.DisableMovementControl();
            GameController.OnLevelFail();
        }

        public static string GetCurrentAreaText()
            => $"AREA {ActiveRoom.CurrentWorldIndex + 1}-{ActiveRoom.CurrentLevelIndex + 1}";

        public static List<int> SplitIntEqually(int value, int partsAmount)
        {
            var floatPart = (float) value / partsAmount;
            var part = Mathf.FloorToInt(floatPart);

            var result = new List<int>();
            if (partsAmount <= 0) return result;
            var sum = 0;

            for (var i = 0; i < partsAmount; i++)
            {
                result.Add(part);
                sum += part;
            }

            if (sum < value)
                result[^1] += value - sum;

            return result;
        }

        #region Dev

        public static void NextLevelDev()
        {
            needCharacterSugession = false;
            IncreaseLevelInSave();
            GameController.OnLevelCompleteClosed();
        }

        public static void PrevLevelDev()
        {
            needCharacterSugession = false;
            DecreaseLevelInSaveDev();
            GameController.OnLevelCompleteClosed();
        }

        static void DecreaseLevelInSaveDev()
        {
            levelSave.LevelIndex--;
            if (levelSave.LevelIndex >= 0) return;
            levelSave.LevelIndex = 0;
            levelSave.WorldIndex--;
            if (levelSave.WorldIndex < 0)
                levelSave.WorldIndex = 0;
        }

        #endregion
    }
}