using System;
using UnityEngine;

namespace MobileTools.Leaderboards.Code
{
    public class LeaderboardPlayer : MonoBehaviour
    {
        // public PlayerExperience exp;
        const string PlayerScoreKey = "PlayerScore";
        public int Score => file.score;
        public PlayerScoreFile file;

        void Awake()
        {
            Load();
        }


        void Start()
        {
            // EventManager.Instance.OnExpAdded += Add;
        }

        void OnDisable()
        {
            // if (!EventManager.Instance) return;
            //  EventManager.Instance.OnExpAdded -= Add;
        }
        public event Action<float> OnChange = delegate { };
        void Add(Vector3 vector, float exp)
        {

            file.score += (int)exp;
            Save();
            OnChange(file.score);
        }

        public void Reset()
        {
            file.score = 0;
            Save();
        }


        public void Save()
        {
            var jsonData = JsonUtility.ToJson(file);
            PlayerPrefs.SetString(PlayerScoreKey, jsonData);
        }


        public void Load()
        {
            file = PlayerPrefs.HasKey(PlayerScoreKey)
                ? JsonUtility.FromJson<PlayerScoreFile>(PlayerPrefs.GetString(PlayerScoreKey))
                : new PlayerScoreFile(0, 0);
        }





    }
}
