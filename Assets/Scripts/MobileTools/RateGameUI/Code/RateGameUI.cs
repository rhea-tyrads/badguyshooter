using System.Collections.Generic;
using MobileTools.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MobileTools.RateGameUI.Code
{
    public class RateGameUI : MobilePopupUI
    {
        [Space(20)] public string googleStoreLink;
        public string email;

        [Space(20)] public List<RateButtonUI> stars = new();
        public Button sendFeedbackButton;
        public Button hideFeedbackButton;
        public Sprite starActive;
        public Sprite starInactive;
        public Transform ratePanel;
        public Transform feedbackPanel;
        public TMP_InputField feedbackInput;
        // SetupSO SO => Game.Instance.gameplay.so;

        void Start()
        {
            for (var i = 0; i < stars.Count; i++)
            {
                var star = stars[i];
                star.icon.sprite = starInactive;
                star.id = i;
            }
        }

        void StarsClicked(RateButtonUI star)
        {
            for (var i = 0; i < star.id + 1; i++)
            {
                var s = stars[i];
                var t = s.transform;
                //   t.DOScale(1.1f, 0.2f).OnComplete(() => t.DOScale(1f, 0.2f));
                s.icon.sprite = starActive;
            }

            Invoke(star.id < 3 ? nameof(Feedback) : nameof(Review), 1f);

            Keys.Rate();
        }

        void Feedback()
        {
            var t = feedbackPanel;
            // t.DOScale(1.1f, 0.2f).OnComplete(() => t.DOScale(1f, 0.2f));
            feedbackPanel.gameObject.SetActive(true);
            ratePanel.gameObject.SetActive(false);
            sendFeedbackButton.onClick.AddListener(SendFeedback);
            hideFeedbackButton.onClick.AddListener(Hide);
        }

        void SendFeedback()
        {
            sendFeedbackButton.onClick.RemoveListener(SendFeedback);
            var subject = "SoloShooter3D Feedback";
            var uri = "mailto:" + email + "?subject=" + subject + "&body=" + feedbackInput.text;
            Application.OpenURL(uri);
            Invoke(nameof(Hide), 0.4f);
        }

        void Review()
        {
            Application.OpenURL(googleStoreLink);
            Hide();
        }

        protected override void Showing()
        {
            feedbackPanel.gameObject.SetActive(false);
            SubscribeStars();
        }

        protected override void Hiding()
        {
            UnsubscribeStars();
        }

        void SubscribeStars()
        {
            foreach (var star in stars)
                star.OnClick += StarsClicked;
        }

        void UnsubscribeStars()
        {
            foreach (var star in stars)
                star.OnClick -= StarsClicked;
        }
    }
}