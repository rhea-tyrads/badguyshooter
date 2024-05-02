using System;

namespace Utilities.Extensions
{
    public static class FuncExtensions
    {
        public static T Please<T>(this T self, Action<T> set)
        {
            set.Invoke(self);
            return self;
        }

        public static T Please<T>(this T self, Action<T> apply, Func<bool> when)
        {
            if (when())
                apply?.Invoke(self);

            return self;
        }

        public static T Please<T>(this T self, Action<T> apply, bool when)
        {
            if (when)
                apply?.Invoke(self);

            return self;
        }
    }
}