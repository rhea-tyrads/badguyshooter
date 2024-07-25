using com.adjust.sdk;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class ExperienceController : MonoBehaviour
    {
        static int FloatingTextHash => FloatingTextController.GetHash("Stars");
        static int SaveHash => "Experience".GetHashCode();
        [SerializeField] ExperienceDatabase database;
        static ExperienceController _instance;
        ExperienceSave _save;
        ExperienceUIController _ui;

        public static int CurrentLevel
        {
            get => _instance._save.CurrentLevel;
            private set => _instance._save.CurrentLevel = value;
        }

        public static int ExperiencePoints
        {
            get => _instance._save.CurrentExperiencePoints;
            private set => _instance._save.CurrentExperiencePoints = value;
        }

        public ExperienceLevelData CurrentLevelData => database.GetData(CurrentLevel);
        public ExperienceLevelData NextLevelData => database.GetData(CurrentLevel + 1);

        public static event SimpleCallback OnExperienceGained;
        public static event SimpleCallback OnLevelIncreased;

        void Awake()
        {
            _instance = this;
        }

        public void Initialise()
        {
            _save = SaveController.GetSaveObject<ExperienceSave>(SaveHash);
            database.Init();
            _ui = UIController.GetPage<UIMainMenu>().ExperienceUIController;
            _ui.Init(this);
        }

        public static void Add(int amount)
        {
            _instance.AddXp(amount);
        }

        void AddXp(int amount)
        {
            ExperiencePoints += amount;
            FloatingTextController.Spawn(FloatingTextHash, $"+{amount}",
                CharacterBehaviour.Transform.position + new Vector3(3, 6, 0), Quaternion.identity, 1f);
           
            _ui.PlayXpGainedAnimation(amount, CharacterBehaviour.Transform.position,
                () => { _ui.UpdateUI(false); });

            // new level reached
            if (ExperiencePoints >= NextLevelData.ExperienceRequired)
            {
                CurrentLevel++;
                SendAdjustEvent();
                OnLevelIncreased?.Invoke();
            }

            OnExperienceGained?.Invoke();
        }

        void SendAdjustEvent()
        {
            var token = CurrentLevel switch
            {
                2 => "cqttkc",
                3 => "lw3yhu",
                4 => "sekqc8",
                5 => "4krrs0",
                6 => "5qn3hh",
                7 => "3hhvlg",
                _ => string.Empty
            };

            var send = new AdjustEvent(token);
            Adjust.trackEvent(send);
        }

        public static int XpRequiredForLevel(int level)
            => _instance.database.GetData(level).ExperienceRequired;

        #region Development

        public static void SetLevelDev(int level)
        {
            CurrentLevel = level;
            ExperiencePoints = _instance.database.GetData(level).ExperienceRequired;
            //instance.expUI.UpdateUI(true);
            OnLevelIncreased?.Invoke();
        }

        #endregion
    }
}