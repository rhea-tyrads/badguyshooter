// -------------------------------------------------------------------------------------------
// The MIT License
// MonoCache is a fast optimization framework for Unity https://github.com/MeeXaSiK/MonoCache
// Copyright (c) 2021-2022 Night Train Code
// -------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using MobileTools.MonoCache.Interfaces;
using MobileTools.MonoCache.System;
using UnityEngine;

namespace MobileTools.MonoCache
{
    [DisallowMultipleComponent]
    public sealed class GlobalUpdate : Singleton<GlobalUpdate>
    {
        public event Action OnUpdate;
        public event Action OnFixedUpdate;
        public event Action OnLateUpdate;

        public const string OnEnableMethodName = "OnEnable";
        public const string OnDisableMethodName = "OnDisable";
        
        public const string UpdateMethodName = nameof(Update);
        public const string FixedUpdateMethodName = nameof(FixedUpdate);
        public const string LateUpdateMethodName = nameof(LateUpdate);

        public readonly List<IRunSystem> RunSystems = new(1024);
        public readonly List<IFixedRunSystem> FixedRunSystems = new(512);
        public readonly List<ILateRunSystem> LateRunSystems = new(256);

        readonly MonoCacheExceptionsChecker monoCacheExceptionsChecker = new();

        void Awake()
        {
            monoCacheExceptionsChecker.CheckForExceptions();
        }

        void Update()
        {
            for (var i = 0; i < RunSystems.Count; i++)
            {
                RunSystems[i].OnRun();
            }
            
            OnUpdate?.Invoke();
        }

        void FixedUpdate()
        {
            for (var i = 0; i < FixedRunSystems.Count; i++)
            {
                FixedRunSystems[i].OnFixedRun();
            }

            OnFixedUpdate?.Invoke();
        }

        void LateUpdate()
        {
            for (var i = 0; i < LateRunSystems.Count; i++)
            {
                LateRunSystems[i].OnLateRun();
            }

            OnLateUpdate?.Invoke();
        }
    }
}