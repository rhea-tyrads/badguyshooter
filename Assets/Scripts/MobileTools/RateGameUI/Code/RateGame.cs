using MobileTools.SDK;
using MobileTools.Utilities;
using Watermelon.LevelSystem;

namespace MobileTools.RateGameUI.Code
{
    public class RateGame : MobileTool
    {
        public RateGameUI ui;

        void Start()
        {
            ui.Hide();
            if (Keys.IsRated) return;
            SDKEvents.Instance.OnLevelComplete += LevelComplete;
        }

        void LevelComplete(int arg1, int arg2)
        {
            if (Keys.IsRated) return;
            var level = ActiveRoom.Level + 1;
            if (level % 7 == 0) Show();
        }

        public void Show()
        {
            ui.Show();
        }
    }
}