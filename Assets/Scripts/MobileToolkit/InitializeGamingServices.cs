using System;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Purchasing.Core.InitializeGamingServices
{
    public class InitializeGamingServices : MonoBehaviour
    {
        const string k_Environment = "production";

        void Awake()
        {
            Initialize(OnSuccess, OnError);
        }

        void Start()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                var text =
                    "Error: Unity Gaming Services not initialized.\n" +
                    "To initialize Unity Gaming Services, open the file \"InitializeGamingServices.cs\" " +
                    "and uncomment the line \"Initialize(OnSuccess, OnError);\" in the \"Awake\" method.";
                Debug.LogError(text);
            }
        }

        void Initialize(Action onSuccess, Action<string> onError)
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(k_Environment);

                UnityServices.InitializeAsync(options).ContinueWith(task => onSuccess());
            }
            catch (Exception exception)
            {
                onError(exception.Message);
            }
        }

        void OnSuccess()
        {
            var text = "Congratulations!\nUnity Gaming Services has been successfully initialized.";
            Debug.Log(text);
        }

        void OnError(string message)
        {
            var text = $"Unity Gaming Services failed to initialize with error: {message}.";
            Debug.LogError(text);
        }


    }
}
