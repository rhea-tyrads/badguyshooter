using UnityEngine;

namespace MobileTools.Android
{
    public class AndroidSettings : MonoBehaviour
    {
        public bool NeverSleepScreen;
        public bool BackButtonQuit;
        public bool DisableDebugger;
        public GameObject debugConsole;

        void Start()
        {
            Init();
        }


        void Init()
        {
            if (NeverSleepScreen) Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (!BackButtonQuit) enabled = false;

            if (DisableDebugger)
            {
                Debug.Log("Debugger disabled for performance boost!");
                DisableDebug();
            }
            else
            {
                Debug.LogError("!!!!!!!!!!!!!!!!!!!!!Please, disable debugger for performance boost!!!!!!!!!!!!!!");
                EnableDebug();
            }

#if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
#endif
        }

        void EnableDebug()
        {
            Debug.unityLogger.logEnabled = true;
            if (debugConsole) debugConsole.SetActive(true);
        }

        void DisableDebug()
        {
            Debug.unityLogger.logEnabled = false;
            if (debugConsole) debugConsole.SetActive(false);
        }

        void Update()
        {
            return;
            if (Application.platform != RuntimePlatform.Android) return;
            if (!Input.GetKey(KeyCode.Escape)) return;
            Application.Quit();
        }
    }
}