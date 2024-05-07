using System;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ButtonAttribute : DrawerAttribute
    {
        string text;
        public string Text => text;

        object[] methodParams;
        public object[] Params => methodParams;

        public ButtonAttribute(string text = null, params object[] methodParams)
        {
            this.text = text;
            this.methodParams = methodParams;
        }
    }
}
