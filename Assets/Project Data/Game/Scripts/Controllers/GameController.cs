using System;
using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        static GameController instance;

        [Header("Refferences")] [SerializeField]
        UIController uiController;

        public BonusController bonuses;

        [Space] [DrawReference] [SerializeField]
        GameSettings settings;

        CurrenciesController currenciesController;
        UpgradesController upgradesController;
        ParticlesController particlesController;
        FloatingTextController floatingTextController;
        ExperienceController experienceController;
        WeaponsController weaponsController;
        CharactersController charactersController;
        BalanceController balanceController;
        EnemyController enemyController;
        TutorialController tutorialController;

        public static GameSettings Settings => instance.settings;
        public static bool IsGameActive { get; private set; }

        public static bool IsDoubleReward;

        void Awake()
        {
            instance = this;
            SaveController.Initialise(false);

            // Cache components
            CacheComponent(out currenciesController);
            CacheComponent(out upgradesController);
            CacheComponent(out particlesController);
            CacheComponent(out floatingTextController);
            CacheComponent(out experienceController);
            CacheComponent(out weaponsController);
            CacheComponent(out charactersController);
            CacheComponent(out balanceController);
            CacheComponent(out enemyController);
            CacheComponent(out tutorialController);
        }

        void Start()
        {
            InitialiseGame();
            Invoke(nameof(CheckOldUser), 0.2f);
        }

        void CheckOldUser()
        {
            LevelController.CheckOldUser();
        }

        public void InitialiseGame()
        {
            CustomMusicController.Initialise(AudioController.Music.menuMusic);

            uiController.Initialise();
            currenciesController.Initialise();
            tutorialController.Initialise();
            upgradesController.Initialise();
            particlesController.Initialise();
            floatingTextController.Inititalise();
            settings.Initialise();

            LevelController.Initialise();

            experienceController.Initialise();
            weaponsController.Initialise();
            charactersController.Initialise();
            balanceController.Initialise();
            enemyController.Initialise();

            LevelController.SpawnPlayer();
            uiController.InitialisePages();
            UIController.ShowPage<UIMainMenu>();
            CameraController.SetCameraShiftState(false);
            LevelController.LoadCurrentLevel();
        }

        public static void OnGameStarted()
        {
            IsGameActive = true;
        }


        public static void LevelComplete()
        {
            if (!IsGameActive) return;

            var levelData = LevelController.CurrentLevelData;
            var completePage = UIController.GetPage<UIComplete>();
            var world = ActiveRoom.World + 1;
            var level = ActiveRoom.Level + 1;
            var money = levelData.GetCoinsReward();
            var experience = levelData.XPAmount;
            var cards = levelData.GetCardsReward();
            completePage.SetData(world, level, money, experience, cards);

            //Debug.LogError("COMPLETE: lvl" + ActiveRoom.CurrentLevelIndex
            //+ ", world: " + ActiveRoom.CurrentWorldIndex + ", " + currentLevel.Rooms
            //+ ", total levels here: " + currentLevel.Rooms.Length);

            UIController.OnPageOpenedEvent += OnCompletePageOpened;
            instance.weaponsController.CheckWeaponUpdateState();
            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();
            IsGameActive = false;
        }

        static void OnCompletePageOpened(UIPage page, System.Type pageType)
        {
            if (pageType != typeof(UIComplete)) return;
            LevelController.UnloadLevel();
            UIController.OnPageOpenedEvent -= OnCompletePageOpened;
            AdsManager.ShowInterstitial(null);
        }

        public static void OnLevelCompleteClosed()
        {
            UIController.HidePage<UIComplete>(() =>
            {
                if (LevelController.NeedCharacterSugession)
                    UIController.ShowPage<UICharacterSuggestion>();
                else
                    ShowMainMenu();
            });
        }

        public static void OnCharacterSuggestionClosed()
        {
            ShowMainMenu();
        }

        static void ShowMainMenu()
        {
            var xp = LevelController.CurrentLevelData.XPAmount;
            if (IsDoubleReward)
            {
                IsDoubleReward = false;
                xp *= 2;
                WeaponsController.AddCards(LevelController.cardRewards);
            }

            CustomMusicController.ToggleMusic(AudioController.Music.menuMusic, 0.3f, 0.3f);
            CameraController.SetCameraShiftState(false);
            CameraController.EnableCamera(CameraType.Menu);
            UIController.ShowPage<UIMainMenu>();
            ExperienceController.Add(xp);
            SaveController.Save(true);
            LevelController.LoadCurrentLevel();
        }

        public static void OnLevelExit()
        {
            IsGameActive = false;
        }

        public static void OnLevelFail()
        {
            if (!IsGameActive) return;

            if (LevelController.characterBehaviour.respawnCount <= 0)
            {
                UIController.HidePage<UIGame>(() =>
                {
                    UIController.ShowPage<UIGameOver>();
                    UIController.OnPageOpenedEvent += OnFailedPageOpened;
                });
            }

            LevelController.OnLevelFailed();
            IsGameActive = false;
        }

        static void OnFailedPageOpened(UIPage page, System.Type pageType)
        {
            if (pageType != typeof(UIGameOver)) return;
            //   AdsManager.ShowInterstitial(null);
            UIController.OnPageOpenedEvent -= OnFailedPageOpened;
        }

        public static void OnReplayLevel()
        {
            IsGameActive = true;
            CustomMusicController.ToggleMusic(AudioController.Music.menuMusic, 0.3f, 0.3f);
            CameraController.SetCameraShiftState(false);
            CameraController.EnableCamera(CameraType.Menu);
            LevelController.UnloadLevel();
            UIController.HidePage<UIGameOver>(() =>
            {
                LevelController.LoadCurrentLevel();
                UIController.ShowPage<UIMainMenu>();
            });
        }

        public static void OnRevive()
        {
            IsGameActive = true;
            UIController.HidePage<UIGameOver>(() =>
            {
                LevelController.ReviveCharacter();
                UIController.ShowPage<UIGame>();
            });
        }

        bool CacheComponent<T>(out T component) where T : Component
        {
            var unboxedComponent = gameObject.GetComponent(typeof(T));
            if (unboxedComponent != null)
            {
                component = (T)unboxedComponent;
                return true;
            }

            Debug.LogError($"Scripts Holder doesn't have {typeof(T)} script added to it");
            component = null;
            return false;
        }
    }
}