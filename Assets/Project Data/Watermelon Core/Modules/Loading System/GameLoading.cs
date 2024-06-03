using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Watermelon
{
    public static class GameLoading
    {
        const float MINIMUM_LOADING_TIME = 2.0f;

        static AsyncOperation loadingOperation;

        static bool isReadyToHide;
        static bool manualControlMode;

        static string loadingMessage;
        static List<LoadingTask> loadingTasks = new();

        public static event LoadingCallback OnLoading;
        public static event SimpleCallback OnLoadingFinished;

        public static void SetLoadingMessage(string message)
        {
            loadingMessage = message;

            var progress = 0.0f;
            if (loadingOperation != null)
                progress = loadingOperation.progress;

            OnLoading(progress, message);
        }

        public static void AddTask(LoadingTask loadingTask)
        {
            loadingTasks.Add(loadingTask);
        }

        static IEnumerator LoadSceneCoroutine(SimpleCallback onSceneLoaded = null)
        {
            isReadyToHide = false;

            var realtimeSinceStartup = Time.realtimeSinceStartup;

            var taskIndex = 0;
            while(taskIndex < loadingTasks.Count)
            {
                if(!loadingTasks[taskIndex].IsActive)
                    loadingTasks[taskIndex].Activate();

                if (loadingTasks[taskIndex].IsFinished)
                {
                    taskIndex++;
                }

                yield return null;
            }

            var sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (SceneManager.sceneCount < sceneIndex)
                Debug.LogError("[Loading]: First scene is missing!");

            var minimumFinishTime = realtimeSinceStartup + MINIMUM_LOADING_TIME;

            loadingOperation = SceneManager.LoadSceneAsync(sceneIndex);
            loadingOperation.allowSceneActivation = false;

            while (!loadingOperation.isDone || realtimeSinceStartup < minimumFinishTime)
            {
                yield return null;

                realtimeSinceStartup = Time.realtimeSinceStartup;

                OnLoading?.Invoke(1.0f, loadingMessage);

                if (loadingOperation.progress >= 0.9f)
                {
                    loadingOperation.allowSceneActivation = true;
                }
            }

            if(manualControlMode)
            {
                // Debug check if MarkAsReadyToHide is implemented
                Tween.DelayedCall(10, () =>
                {
                    if (!isReadyToHide)
                        Debug.LogError("[Loading]: Seems like you forget to call MarkAsReadyToHide method to finish the loading process.");
                });

                while (!isReadyToHide)
                {
                    yield return null;
                }
            }

            OnLoading?.Invoke(1.0f, "Done");

            yield return null;

            if (onSceneLoaded != null)
                onSceneLoaded.Invoke();

            OnLoadingFinished?.Invoke();
        }

        static IEnumerator SimpleLoadCoroutine(SimpleCallback onSceneLoaded = null)
        {
            var realtimeSinceStartup = Time.realtimeSinceStartup;

            var taskIndex = 0;
            while (taskIndex < loadingTasks.Count)
            {
                if (!loadingTasks[taskIndex].IsActive)
                    loadingTasks[taskIndex].Activate();

                if (loadingTasks[taskIndex].IsFinished)
                {
                    taskIndex++;
                }

                yield return null;
            }

            if (onSceneLoaded != null)
                onSceneLoaded.Invoke();

            OnLoadingFinished?.Invoke();
        }

        public static void MarkAsReadyToHide()
        {
            isReadyToHide = true;
        }

        public static void EnableManualControlMode()
        {
            manualControlMode = true;
        }

        public static void LoadGameScene(SimpleCallback onSceneLoaded = null)
        {
            SetLoadingMessage("Loading..");

            Tween.InvokeCoroutine(LoadSceneCoroutine(onSceneLoaded));
        }

        public static void SimpleLoad(SimpleCallback onSceneLoaded = null)
        {
            Tween.InvokeCoroutine(SimpleLoadCoroutine(onSceneLoaded));
        }

        public delegate void LoadingCallback(float state, string message);
    }
}

// -----------------
// Initialiser v 0.2
// -----------------

// Changelog
// v 0.2
// � Loading graphics removed from Init scene
// � Added GameLoadingSettings Init Module
// � Added manual control mode (If manual mode is enabled, the loading screen will be active until GameLoading.MarkAsReadyToHide method has been called)
// v 0.1
// � Added basic version