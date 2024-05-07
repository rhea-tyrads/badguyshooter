using System;
using System.Collections;
using UnityEngine;
using System.Threading;

namespace Watermelon
{
    public static class SaveController
    {
        const string SAVE_FILE_NAME = "save";
        const int SAVE_DELAY = 30;

        static GlobalSave globalSave;

        static bool isSaveLoaded;
        public static bool IsSaveLoaded => isSaveLoaded;

        static bool isSaveRequired;

        public static int LevelId { get => globalSave.LevelId; set => globalSave.LevelId = value; }
        public static float GameTime => globalSave.GameTime;

        public static DateTime LastExitTime => globalSave.LastExitTime;

        public static event SimpleCallback OnSaveLoaded;

        public static void Initialise(bool useAutoSave, bool clearSave = false, float overrideTime = -1f)
        {
            if (clearSave)
            {
                InitClear(overrideTime != -1f ? overrideTime : Time.time);
            }
            else
            {
                Load(overrideTime != -1f ? overrideTime : Time.time);
            }

            if (useAutoSave)
            {
                // Enable auto-save coroutine
                Tween.InvokeCoroutine(AutoSaveCoroutine());
            }
        }

        public static void UpdateTime(float time)
        {
            globalSave.Time = time;
        }

        public static T GetSaveObject<T>(int hash) where T : ISaveObject, new()
        {
            if (!isSaveLoaded)
            {
                Debug.LogError("Save controller has not been initialized");
                return default;
            }

            return globalSave.GetSaveObject<T>(hash);
        }

        public static T GetSaveObject<T>(string uniqueName) where T : ISaveObject, new()
        {
            return GetSaveObject<T>(uniqueName.GetHashCode());
        }

        static void InitClear(float time)
        {
            globalSave = new GlobalSave();
            globalSave.Init(time);

            Debug.Log("[Save Controller]: Created clear save!");

            isSaveLoaded = true;
        }

        static void Load(float time)
        {
            if (isSaveLoaded)
                return;

            // Try to read and deserialize file or create new one
            globalSave = BaseSaveWrapper.ActiveWrapper.Load(SAVE_FILE_NAME);

            globalSave.Init(time);

            Debug.Log("[Save Controller]: Save is loaded!");

            isSaveLoaded = true;

            OnSaveLoaded?.Invoke();
        }

        public static void Save(bool forceSave = false)
        {
            if (!forceSave && !isSaveRequired) return;
            if (globalSave == null) return;

            globalSave.Flush();

            var saveWrapper = BaseSaveWrapper.ActiveWrapper;
            if(saveWrapper.UseThreads())
            {
                var saveThread = new Thread(() => BaseSaveWrapper.ActiveWrapper.Save(globalSave, SAVE_FILE_NAME));
                saveThread.Start();
            }
            else
            {
                BaseSaveWrapper.ActiveWrapper.Save(globalSave, SAVE_FILE_NAME);
            }

            Debug.Log("[Save Controller]: Game is saved!");

            isSaveRequired = false;
        }

        public static void SaveCustom(GlobalSave globalSave)
        {
            if(globalSave != null)
            {
                globalSave.Flush();

                BaseSaveWrapper.ActiveWrapper.Save(globalSave, SAVE_FILE_NAME);
            }
        }

        public static void MarkAsSaveIsRequired()
        {
            isSaveRequired = true;
        }

        static IEnumerator AutoSaveCoroutine()
        {
            var waitForSeconds = new WaitForSeconds(SAVE_DELAY);

            while (true)
            {
                yield return waitForSeconds;

                Save();
            }
        }

        public static void PresetsSave(string fullFileName)
        {
            globalSave.Flush();

            BaseSaveWrapper.ActiveWrapper.Save(globalSave, fullFileName);
        }

        public static void Info()
        {
            globalSave.Info();
        }

        public static void DeleteSaveFile()
        {
            BaseSaveWrapper.ActiveWrapper.Delete(SAVE_FILE_NAME);
        }

        public static GlobalSave GetGlobalSave()
        {
            var tempGlobalSave = BaseSaveWrapper.ActiveWrapper.Load(SAVE_FILE_NAME);

            tempGlobalSave.Init(Time.time);

            return tempGlobalSave;
        }
    }
}