using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class OnValueChangedAttribute : MetaAttribute
    {
        string callbackName;

        public OnValueChangedAttribute(string callbackName)
        {
            this.callbackName = callbackName;
        }

        public string CallbackName
        {
            get
            {
                return this.callbackName;
            }
        }
    }
}