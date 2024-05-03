using System;
 

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
        DateTime now = DateTime.Now;
        int today = (int)now.DayOfWeek;  
        bool[] daysSelected = { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };

        for (int i = 1; i <= 7; i++)
        {
            int nextDayIndex = (today + i) % 7;
            if (daysSelected[nextDayIndex])
            {
                DateTime nextSelectedDay = now.AddDays(i);
                nextSelectedDay = nextSelectedDay.Date;
                return nextSelectedDay - now;
            }
        }

        return TimeSpan.Zero;
    }
}
