using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
