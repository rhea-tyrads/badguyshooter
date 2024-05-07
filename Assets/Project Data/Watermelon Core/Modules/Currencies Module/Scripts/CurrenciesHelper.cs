using UnityEngine;

namespace Watermelon
{
    public static class CurrenciesHelper
    {
        static readonly string[] DIGITS = new string[] { "", "K", "M", "B", "T", "Qa" };

        public static string Format(int value)
        {
            float moneyRepresentation = value;
            var counter = 0;

            while (moneyRepresentation >= 1000)
            {
                moneyRepresentation /= 1000;
                counter++;
            }

            if (moneyRepresentation >= 100)
            {
                moneyRepresentation = Mathf.Floor(moneyRepresentation);
                if (counter != 0)
                    return moneyRepresentation.ToString("F0") + GetDigits(counter);
            }
            else if (moneyRepresentation >= 10)
            {
                var result = moneyRepresentation.ToString("F1");

                if (result[result.Length - 1] == '0')
                    result = result.Remove(result.Length - 2);

                if (counter != 0)
                    return result + GetDigits(counter);
            }
            else
            {
                var result = moneyRepresentation.ToString("F2");

                if (result[result.Length - 1] == '0')
                {
                    result = result.Remove(result.Length - 1);

                    if (result[result.Length - 1] == '0')
                        result = result.Remove(result.Length - 2);
                }

                if (counter != 0)
                    return result + GetDigits(counter);
            }

            return Mathf.RoundToInt(moneyRepresentation).ToString();
        }

        static string GetDigits(int index)
        {
            if (index < 0 || index >= DIGITS.Length)
                return "";

            return DIGITS[index];
        }
    }
}