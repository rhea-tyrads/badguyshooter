using MobileTools.DailyLogins.Code;
using UnityEngine;

namespace MobileTools.SignalsUI.SignalConditions
{
    public class SignalConditionDailyLogin : SignalCondition
    {

        public DailyLoginDayUI dayUI;
        void Start()
        {
            if (!dayUI) dayUI = GetComponentInParent<DailyLoginDayUI>();
            dayUI.OnRefresh += RefreshSt;
            RefreshSt();
        }

        void RefreshSt()
        {
            if (IsFirstLaunch())
            {
                ForceHide(true);
                return;
            }

            ForceHide(!dayUI.IsCollectable);
        }

        bool IsFirstLaunch()
        {
            if (PlayerPrefs.HasKey(gameObject.name)) return false;
            PlayerPrefs.SetInt(gameObject.name, 1);
            return true;
        }
    }
}
