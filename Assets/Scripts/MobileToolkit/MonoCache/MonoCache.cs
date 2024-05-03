// -------------------------------------------------------------------------------------------
// The MIT License
// MonoCache is a fast optimization framework for Unity https://github.com/MeeXaSiK/MonoCache
// Copyright (c) 2021-2022 Night Train Code
// -------------------------------------------------------------------------------------------
/*
 *   protected override void OnEnabled()
    {
        //Replaces base "OnEnable()" method
    }

    protected override void OnDisabled()
    {
        //Replaces base "OnDisable()" method
    }

    protected override void OnUpdate()
    {
        //Replaces base "Update()" method
    }

    protected override void OnFixedUpdate()
    {
        //Replaces base "FixedUpdate()" method
    }

    protected override void OnLateUpdate()
    {
        //Replaces base "LateUpdate()" method
    }
*/

using UnityEngine;
using Utilities.MonoCache.Interfaces;
using Utilities.MonoCache.System;

namespace Utilities.MonoCache
{
    public abstract class MonoCache : MonoAllocation, IRunSystem, IFixedRunSystem, ILateRunSystem
    {
        GlobalUpdate _globalUpdate;
        bool _isSetup;

        void OnEnable()
        {
            OnEnabled();

            if (_isSetup == false)
            {
                Setup();
            }
            
            _globalUpdate.RunSystems.Add(this);
            _globalUpdate.FixedRunSystems.Add(this);
            _globalUpdate.LateRunSystems.Add(this);
        }

        void OnDisable()
        {
            _globalUpdate.RunSystems.Remove(this);
            _globalUpdate.FixedRunSystems.Remove(this);
            _globalUpdate.LateRunSystems.Remove(this);

            OnDisabled();
        }

        void Setup()
        {
            if (Application.isPlaying)
            {
                _globalUpdate = Singleton<GlobalUpdate>.Instance;
            }
            
            _isSetup = true;
        }
        
        void IRunSystem.OnRun() => OnUpdate();
        void IFixedRunSystem.OnFixedRun() => OnFixedUpdate();
        void ILateRunSystem.OnLateRun() => OnLateUpdate();

        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        
        protected virtual void OnUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnLateUpdate() { }
    }
}
