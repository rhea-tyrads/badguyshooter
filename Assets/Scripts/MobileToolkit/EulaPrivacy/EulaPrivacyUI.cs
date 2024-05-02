using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoreUI
{
    public class EulaPrivacyUI : MonoBehaviour
    {
        public ScreenUI popup;
        public Button acceptButton;
        public Button eulaURLButton;
        public string url;
        public GameObject loadingCanvas;

        const string KEY = "EULA_ACCEPTED";

        void Start()
        {
            var accepted = PlayerPrefs.HasKey(KEY);
            if (accepted)
            {
                loadingCanvas.SetActive(false);
                Hide();
            }
            else
            {
                loadingCanvas.SetActive(true);
                Show();
            }
        }

        void Show()
        {
            acceptButton.onClick.AddListener(Accept);
            eulaURLButton.onClick.AddListener(OpenURL);
            popup.Show();
        }

        public event Action OnAccept = delegate { };

        void Accept()
        {
            OnAccept();

            acceptButton.onClick.RemoveListener(Hide);
            eulaURLButton.onClick.RemoveListener(OpenURL);

            PlayerPrefs.SetInt(KEY, 1);
            PlayerPrefs.Save();

            Hide();

            //var conrt = LevelsController.Instance;
            //conrt.stageSelector.uis[0].Click();
            //conrt.Play();
        }

        public void Hide()
        {
            popup.Hide();
        }

        void OpenURL()
        {
            Application.OpenURL(url);
        }
    }
}