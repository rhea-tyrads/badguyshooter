using System;
using System.Collections.Generic;
using MobileTools.RateGameUI.Code;
using TMPro;


namespace MobileTools.DailyLogins.Code
{
    public class DailyLoginUI : MobilePopupUI
    {
        public List<DailyLoginDayUI> list = new();
        public event Action<int> OnCollect = delegate { };
        //  Sprite GetIcon(RewardEnum type) => Game.Instance.gameplay.so.icons.GetIcon(type);
        public TextMeshProUGUI week;
        public TextMeshProUGUI message;

        public void Init(List<DailyReward> rewards)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var day = list[i];
                day.SetDayNumber(i + 1);
                day.OnCollect += Collect;

                var reward = rewards[i];
                //  var icon = GetIcon(reward.type);
                // day.SetRewardIcon(icon);
                day.SetRewardAmount(reward.amount);
            }

            var r = UnityEngine.Random.Range(0, welcomeMessages.Length);
            message.text = welcomeMessages[r];
        }

        public void SetState(int logins, int collected, int currentWeek)
        {
            week.text = "Week " + currentWeek;

            foreach (var day in list)
            {
                day.DisableHighlight();
                day.NotCollected();
                day.HideTommorow();
            }

            for (var i = 0; i < logins; i++)
            {
                list[i].Highlight();
            }

            for (var i = 0; i < collected; i++)
            {
                list[i].DisableHighlight();
                list[i].Collected();
            }

            if (logins < list.Count && collected >= logins)
            {
                if (currentWeek > 1)
                {
                    var nextDay = list[logins];
                    nextDay.ShowTommorow();
                }

            }


        }

        void Collect(int number)
        {
            var id = number - 1;
            OnCollect(id);
        }
        public string[] welcomeMessages = {
            "Welcome aboard, glad you're here!",
            "Hey there! Happy to see you.",
            "Hi! Welcome to the adventure.",
            "Hello! Let's make some magic happen.",
            "Greetings! Excited to have you with us.",
            "Welcome! Ready to explore?",
            "Hey! You're in for a treat.",
            "Hi there! A warm welcome to you.",
            "Hello and welcome! Let's achieve great things.",
            "Welcome to the team! Let's do something great.",
            "Delighted to see you joining us!",
            "Fantastic to have you here!",
            "Cheers to your arrival!",
            "Thrilled to welcome you aboard!",
            "It�s a pleasure to meet you!",
            "Looking forward to our journey together!",
            "What a wonderful surprise! Welcome!",
            "A big welcome from all of us!",
            "So glad you�re here with us!",
            "Welcome � we're thrilled to have you!",
            "Your adventure starts now. Welcome!",
            "The team welcomes you warmly!",
            "A hearty welcome to you!",
            "Welcome � let�s make history together!",
            "Your presence is a real gift!",
            "Welcome � you bring great energy!",
            "So excited to have you on board!",
            "Welcome � you've made a great choice!",
            "Your arrival brings joy!",
            "Welcome � let�s create memorable moments!",
            "Thrilled you�re here! Let�s get started.",
            "Your journey with us begins. Welcome!",
            "Welcome � you fit right in!",
            "Eager to see your contributions!",
            "Your new adventure begins now!",
            "Welcome � you're a fantastic addition!",
            "Your presence excites us!",
            "Delighted to welcome you to the family!",
            "A joyous welcome to you!",
            "Your arrival marks a new chapter!",
            "The beginning of a great experience!",
            "Welcome � a perfect fit for us!",
            "A warm and hearty welcome!",
            "Cheers to new beginnings!",
            "Welcome � you're going to love it here!",
            "So happy you�re joining us!",
            "Your future starts today. Welcome!",
            "Welcome � a great journey awaits!",
            "You�re here! Let�s celebrate!",
            "Welcome � your timing couldn�t be better!"
        };
    }
}
