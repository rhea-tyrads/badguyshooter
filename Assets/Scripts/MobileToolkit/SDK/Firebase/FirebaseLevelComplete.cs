using Firebase.Analytics;
using UnityEngine;

public class FirebaseLevelComplete : MonoBehaviour
{

    void Start()
    {
        // Game.Instance.gameplay.OnVictory -= LevelComplete;
        // Game.Instance.gameplay.OnVictory += LevelComplete;
    }

    void LevelComplete()
    {
        // var lvl = LevelsController.Instance.CurrentLevelNumber;
        var lvl = 0;
        FirebaseAnalytics.LogEvent(Events.LEVEL_COMPLETE, Params.NUMBER, lvl);

        // var param = new[] { new Parameter(Params.NUMBER, lvl) };
        // FirebaseAnalytics.LogEvent(eventName, param);
    }

}
