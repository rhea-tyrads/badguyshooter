#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    [SetupTab("Init Settings", priority = 1, texture = "icon_puzzle")]
    [CreateAssetMenu(fileName = "Project Init Settings", menuName = "Settings/Project Init Settings")]
    [HelpURL("https://docs.google.com/document/d/1ORNWkFMZ5_Cc-BUgu9Ds1DjMjR4ozMCyr6p_GGdyCZk")]
    public class ProjectInitSettings : ScriptableObject
    {
        [SerializeField] InitModule[] coreModules;
        public InitModule[] CoreModules => coreModules;

        [SerializeField] InitModule[] modules;
        public InitModule[] Modules => modules;

        public void Initialise(Initialiser init)
        {
            foreach (var module in coreModules)
            {
                if (module == null) continue;
                module.CreateComponent(init);
            }

            foreach (var module in modules)
            {
                if (module == null) continue;
                module.CreateComponent(init);
            }
        }
    }
}