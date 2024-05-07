using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileTools.DailyLogins.Code
{
    public class DailyLoginDayUI : MonoBehaviour
    {

        public GameObject collected;
        public GameObject highlight;
        public GameObject tommorow;
        public TextMeshProUGUI dayName;

        public Image rewardIcon;
        public TextMeshProUGUI rewardAmount;

        public Button collectButton;
        public event Action<int> OnCollect = delegate { };
        public event Action OnRefresh = delegate { };
        public bool IsCollectable { get; private set; }
        int dayNumber;


        void Awake()
        {
            collectButton.onClick.AddListener(Collect);
        }

        void Collect()
        {

            OnCollect(dayNumber);
        }

        public void SetDayNumber(int number)
        {
            dayNumber = number;
            dayName.text = "Day " + number;
        }

        public void Collected()
        {
            collected.SetActive(true);

        }
        public void NotCollected()
        {
            collected.SetActive(false);
        }
        public void DisableHighlight()
        {
            collectButton.interactable = false;
            IsCollectable = false;
            highlight.SetActive(false);
            OnRefresh();
        }

        public void Highlight()
        {
            collectButton.interactable = true;
            IsCollectable = true;
            highlight.SetActive(true);
            OnRefresh();
        }
        public void ShowTommorow()
        {
            tommorow.SetActive(true);
        }
        public void HideTommorow()
        {
            tommorow.SetActive(false);
        }
        public void SetRewardIcon(Sprite reward)
        {
            rewardIcon.sprite = reward;

        }
        public void SetRewardAmount(int reward)
        {

            rewardAmount.text = reward.ToString();
        }
    }
}
