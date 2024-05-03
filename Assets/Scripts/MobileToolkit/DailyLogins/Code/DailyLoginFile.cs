using FortuneWheels.Scripts;
using System;
using System.Collections.Generic;

[Serializable]
public class DailyLoginFile
{
    public string weekStartDate;
    public int logins;
    public int week;
    public int collected;
    public List<DailyReward> rewards=new();


}