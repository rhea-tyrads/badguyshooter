using Firebase.Analytics;
using UnityEngine;

namespace MobileTools.SDK.Firebase
{
    public class FirebaseFailsPerLevel : MonoBehaviour
    {

        void Start()
        {
            //  Game.Instance.gameplay.OnLose -= Lose;
            //  Game.Instance.gameplay.OnLose += Lose;
        }
        void Lose()
        {
            Invoke(nameof(SendEvent), 0.1f);
        }

        void SendEvent()
        {
            // var lvl = LevelsController.Instance.CurrentLevelNumber;
            var lvl = 0;
            // var fails = LevelsController.Instance.CurrentLevelData.fails;
            var fails = 0;

            Parameter[] parameters =
            {
                new(Params.NUMBER, lvl),
                new(Params.FAILS, fails)
            };

            FirebaseAnalytics.LogEvent(Events.FAILS_PER_LEVEL, parameters);
        }
    }
}
