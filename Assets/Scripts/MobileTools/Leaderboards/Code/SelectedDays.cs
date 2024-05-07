using System;

namespace MobileTools.Leaderboards.Code
{
    [Serializable]
    public class SelectedDays 
    {
        public bool Monday;
        public bool Tuesday;
        public bool Wednesday;
        public bool Thursday;
        public bool Friday;
        public bool Saturday;
        public bool Sunday;

        public TimeSpan GetTimeUntilNextSelectedDay()
        {
            var now = DateTime.Now;
            var today = (int)now.DayOfWeek;  
            bool[] daysSelected = { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };

            for (var i = 1; i <= 7; i++)
            {
                var nextDayIndex = (today + i) % 7;
                if (daysSelected[nextDayIndex])
                {
                    var nextSelectedDay = now.AddDays(i);
                    nextSelectedDay = nextSelectedDay.Date;
                    return nextSelectedDay - now;
                }
            }

            return TimeSpan.Zero;
        }
    }
}
