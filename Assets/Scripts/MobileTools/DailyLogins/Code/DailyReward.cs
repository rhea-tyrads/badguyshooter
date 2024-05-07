using UnityEngine;

namespace MobileTools.DailyLogins.Code
{
    [System.Serializable]
    public class DailyReward
    {
        public DailyRewardType type;
        public int amount;
        [HideInInspector] public Sprite icon;
    }
}