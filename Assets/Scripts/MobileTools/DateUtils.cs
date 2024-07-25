using System;
using System.Globalization;
using UnityEngine;

namespace Utilities
{
    public static class DateUtils
    {
        public static void Save(string key, DateTime value)
            => PlayerPrefs.SetString(key, value.ToString(CultureInfo.InvariantCulture));

        public static DateTime Load(string key)
            => PlayerPrefs.HasKey(key)
                ? DateTime.Parse(PlayerPrefs.GetString(key, string.Empty))
                : DateTime.Now;

        public static TimeSpan TimePassed(string key)
            => DateTime.Now - Load(key);

        public static int SecondsPassed(string key)
            => (int)TimePassed(key).TotalSeconds;
    }
}