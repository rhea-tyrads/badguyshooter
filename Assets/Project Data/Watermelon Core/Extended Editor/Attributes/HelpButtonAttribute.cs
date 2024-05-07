using System;
using UnityEngine;

namespace Watermelon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class HelpButtonAttribute : ExtendedEditorAttribute
    {
        string name;
        public string Name => name;

        string url;
        public string URL => url;

        public HelpButtonAttribute(string name, string url)
        {
            this.name = name;
            this.url = url;
        }

        public void OnButtonClicked()
        {
            if(!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }
    }
}
