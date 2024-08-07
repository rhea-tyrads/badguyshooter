using UnityEngine;

namespace MobileTools.Leaderboards.Code
{
    public class LeaderboardSave : MonoBehaviour
    {
        public LeaderboardFile file;
        const string SaveKey = "LeaderboardSave";
        public void Save()
        {
            var jsonData = JsonUtility.ToJson(file);
            PlayerPrefs.SetString(SaveKey, jsonData);
            PlayerPrefs.Save();
        }


        public void Load()
        {
            file = PlayerPrefs.HasKey(SaveKey)
                ? JsonUtility.FromJson<LeaderboardFile>(PlayerPrefs.GetString(SaveKey))
                : new LeaderboardFile();
        }
    }
}
