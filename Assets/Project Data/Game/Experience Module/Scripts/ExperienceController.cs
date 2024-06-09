using com.adjust.sdk;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class ExperienceController : MonoBehaviour
    {
        static readonly int FLOATING_TEXT_HASH = FloatingTextController.GetHash("Stars");
        static readonly int SAVE_HASH = "Experience".GetHashCode();

        [SerializeField] ExperienceDatabase database;

        static ExperienceController instance;

        ExperienceSave save;
        ExperienceUIController expUI;

        public static int CurrentLevel
        {
            get => instance.save.CurrentLevel;
            private set => instance.save.CurrentLevel = value;
        }

        public static int ExperiencePoints
        {
            get => instance.save.CurrentExperiencePoints;
            private set => instance.save.CurrentExperiencePoints = value;
        }

        public ExperienceLevelData CurrentLevelData => database.GetDataForLevel(CurrentLevel);
        public ExperienceLevelData NextLevelData => database.GetDataForLevel(CurrentLevel + 1);

        public static event SimpleCallback OnExperienceGained;
        public static event SimpleCallback OnLevelIncreased;

        void Awake()
        {
            instance = this;
        }

        public void Initialise()
        {
            save = SaveController.GetSaveObject<ExperienceSave>(SAVE_HASH);
            database.Init();
            expUI = UIController.GetPage<UIMainMenu>().ExperienceUIController;
            expUI.Init(this);
        }

        public static void GainXPPoints(int amount)
        {
            instance.GainExperience(amount);
        }

        public void GainExperience(int amount)
        {
            ExperiencePoints += amount;
            FloatingTextController.SpawnFloatingText(FLOATING_TEXT_HASH, string.Format("+{0}", amount), CharacterBehaviour.Transform.position + new Vector3(3, 6, 0), Quaternion.identity, 1f);
            expUI.PlayXpGainedAnimation(amount, CharacterBehaviour.Transform.position, () =>
            {
                expUI.UpdateUI(false);
            });

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
            => instance.database.GetDataForLevel(level).ExperienceRequired;

        #region Development

        public static void SetLevelDev(int level)
        {
            CurrentLevel = level;
            ExperiencePoints = instance.database.GetDataForLevel(level).ExperienceRequired;
            //instance.expUI.UpdateUI(true);
            OnLevelIncreased?.Invoke();
        }

        #endregion
    }
}
