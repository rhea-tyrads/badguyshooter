using Firebase.Analytics;
using UnityEngine;

namespace MobileTools.SDK.Firebase
{
    public class FirebaseLevelCompleteTime : MonoBehaviour
    {
        float timer;

        void Start()
        {
            // Game.Instance.gameplay.OnVictory -= LevelComplete;
            //  Game.Instance.gameplay.OnVictory += LevelComplete;
        }



        void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

        }

        void LevelComplete()
        {
            //  var lvl = LevelsController.Instance.CurrentLevelNumber;
            var lvl = 0;

            Parameter[] parameters =
            {
                new( Params.NUMBER, lvl),
                new(Params.NUMBER, (int)timer)
            };

            FirebaseAnalytics.LogEvent(Events.LEVEL_COMPLETE_DURATION, parameters);
        }

    }
}
