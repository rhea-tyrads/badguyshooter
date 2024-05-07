using MobileTools.DailyLogins.Code;

namespace MobileTools.SignalsUI.SignalListeners
{
    public class SignalDailyLogin : SignalListener
    {
        public DailyLogin dailyLogin;
        protected override void OnInit()
        {
            dailyLogin.OnCollect += RefreshState;
            RefreshState();
        }

        void RefreshState()
        {
            if (dailyLogin.IsAnyRewardToCollect)
                Active();
            else
                Passive();
        }
    }
}
