using System;
using UnityEngine;

namespace CoreUI
{
    public class EulaPrivacy : MobileTool
    {
        public EulaUI ui;
        public string url;

        const string KEY = "EULA_ACCEPTED";



        void Start()
        {
            var accepted = PlayerPrefs.HasKey(KEY);
            if (accepted)
            {
                //loadingCanvas.SetActive(false);
                Hide();
            }
            else
            {
                //loadingCanvas.SetActive(true);
                Show();
            }
        }

        void Show()
        {

            ui.Show();
            ui.OnURL += OpenURL;
            ui.OnAccept += Accept;
        }

        public event Action OnAccept = delegate { };

        void Accept()
        {
            OnAccept();


            PlayerPrefs.SetInt(KEY, 1);
            PlayerPrefs.Save();

            Hide();

            //var conrt = LevelsController.Instance;
            //conrt.stageSelector.uis[0].Click();
            //conrt.Play();
        }

        public void Hide()
        {
            ui.Hide();
        }

        void OpenURL()
        {
            Application.OpenURL(url);
        }
    }
}