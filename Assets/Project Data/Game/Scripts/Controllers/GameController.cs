using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        static GameController instance;

        [Header("Refferences")]
        [SerializeField] UIController uiController;
        public BonusController bonuses;
        [Space]
        [DrawReference]
        [SerializeField] GameSettings settings;

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

        static bool isGameActive;
        public static bool IsGameActive => isGameActive;

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
            isGameActive = true;
        }

        public AdjustController adjust;
        public static void LevelComplete()
        {
            if (!isGameActive) return;

            var currentLevel = LevelController.CurrentLevelData;
            var completePage = UIController.GetPage<UIComplete>();
            var world = ActiveRoom.CurrentWorldIndex + 1;
            var level = ActiveRoom.CurrentLevelIndex + 1;
            var money = currentLevel.GetCoinsReward();
            var experience = currentLevel.XPAmount;
            var cards = currentLevel.GetCardsReward();
            completePage.SetData(world, level, money, experience, cards);

            //Debug.LogError("COMPLETE: lvl" + ActiveRoom.CurrentLevelIndex
            //+ ", world: " + ActiveRoom.CurrentWorldIndex + ", " + currentLevel.Rooms
            //+ ", total levels here: " + currentLevel.Rooms.Length);
 
            UIController.OnPageOpenedEvent += OnCompletePageOpened;
            instance.weaponsController.CheckWeaponUpdateState();
            UIController.HidePage<UIGame>();
            UIController.ShowPage<UIComplete>();
            isGameActive = false;
        }

 
        static void OnCompletePageOpened(UIPage page, System.Type pageType)
        {
            if (pageType != typeof(UIComplete)) return;
            LevelController.UnloadLevel();
            UIController.OnPageOpenedEvent -= OnCompletePageOpened;
        }

        public static void OnLevelCompleteClosed()
        {
            UIController.HidePage<UIComplete>(() =>
            {
                if (LevelController.NeedCharacterSugession)
                    UIController.ShowPage<UICharacterSuggestion>();
                else
                    ShowMainMenuAfterLevelComplete();
            });
        }

        public static void OnCharacterSugessionClosed()
        {
            ShowMainMenuAfterLevelComplete();
        }

        static void ShowMainMenuAfterLevelComplete()
        {
            AdsManager.ShowInterstitial(null);
            CustomMusicController.ToggleMusic(AudioController.Music.menuMusic, 0.3f, 0.3f);
            CameraController.SetCameraShiftState(false);
            CameraController.EnableCamera(CameraType.Menu);
            UIController.ShowPage<UIMainMenu>();
            ExperienceController.GainXPPoints(LevelController.CurrentLevelData.XPAmount);
            SaveController.Save(true);
            LevelController.LoadCurrentLevel();
        }

        public static void OnLevelExit()
        {
            isGameActive = false;
        }

        public static void OnLevelFail()
        {
            if (!isGameActive) return;

            if (LevelController.characterBehaviour.respawnCount <= 0)
            {
                UIController.HidePage<UIGame>(() =>
                {
                    UIController.ShowPage<UIGameOver>();
                    UIController.OnPageOpenedEvent += OnFailedPageOpened;
                });
            }

            LevelController.OnLevelFailed();
            isGameActive = false;
        }

        static void OnFailedPageOpened(UIPage page, System.Type pageType)
        {
            if (pageType != typeof(UIGameOver)) return;
         //   AdsManager.ShowInterstitial(null);
            UIController.OnPageOpenedEvent -= OnFailedPageOpened;
        }

        public static void OnReplayLevel()
        {
            isGameActive = true;
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
            isGameActive = true;
            UIController.HidePage<UIGameOver>(() =>
            {
                LevelController.ReviveCharacter();
                UIController.ShowPage<UIGame>();
            });
        }

        public bool CacheComponent<T>(out T component) where T : Component
        {
            var unboxedComponent = gameObject.GetComponent(typeof(T));
            if (unboxedComponent != null)
            {
                component = (T) unboxedComponent;
                return true;
            }

            Debug.LogError(string.Format("Scripts Holder doesn't have {0} script added to it", typeof(T)));
            component = null;
            return false;
        }
    }
}