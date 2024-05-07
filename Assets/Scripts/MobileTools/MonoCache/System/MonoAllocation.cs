﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MobileTools.MonoCache.System
{
    public abstract class MonoAllocation : MonoBehaviour
    {
        Dictionary<int, Component> _get;
        Dictionary<int, Component[]> _gets;
        Dictionary<int, Component> _childrenGet;
        Dictionary<int, Component[]> _childrenGets;
        Dictionary<int, Component> _parentGet;
        Dictionary<int, Component[]> _parentGets;
        Dictionary<int, Component> _find;
        Dictionary<int, Component[]> _finds;

        bool allocationEnabled = true;

        public void EnableAllocation()
        {
            allocationEnabled = true;
        }

        public void DisableAllocation()
        {
            allocationEnabled = false;
        }

        public T Get<T>() => GetComponent<T>();
        
        public T[] Gets<T>() => GetComponents<T>();
        
        public T ChildrenGet<T>() => GetComponentInChildren<T>();
        
        public T[] ChildrenGets<T>() => GetComponentsInChildren<T>();
        
        public T ParentGet<T>() => GetComponentInParent<T>();
        
        public T[] ParentGets<T>() => GetComponentsInParent<T>();
        
        public T Find<T>() where T : Object => FindObjectOfType<T>();
        
        public T[] Finds<T>() where T : Object => FindObjectsOfType<T>();
        

        public T GetCached<T>() where T : Component
        {
            return GetSingleCached(_get, GetComponent<T>);
        }

        public T[] GetsCached<T>() where T : Component
        {
            return GetManyCached(_gets, GetComponents<T>);
        }

        public T ChildrenGetCached<T>() where T : Component
        {
            return GetSingleCached(_childrenGet, GetComponentInChildren<T>);
        }

        public T[] ChildrenGetsCached<T>() where T : Component
        {
            return GetManyCached(_childrenGets, GetComponentsInChildren<T>);
        }

        public T ParentGetCached<T>() where T : Component
        {
            return GetSingleCached(_parentGet, GetComponentInParent<T>);
        }

        public T[] ParentGetsCached<T>() where T : Component
        {
            return GetManyCached(_parentGets, GetComponentsInParent<T>);
        }

        public T FindCached<T>() where T : Component
        {
            return GetSingleCached(_find, FindObjectOfType<T>);
        }

        public T[] FindsCached<T>() where T : Component
        {
            return GetManyCached(_finds, FindObjectsOfType<T>);
        }

        T GetSingleCached<T>(Dictionary<int, Component> storage, Func<T> getMethod) where T : Component
        {
            var index = GetInfo<T>.Index;

            if (allocationEnabled)
            {
                storage ??= new Dictionary<int, Component>(16);

                if (storage.TryGetValue(index, out var component))
                {
                    return (T) component;
                }
            }

            var instance = getMethod?.Invoke();

            if (allocationEnabled && instance != null)
            {
                storage.Add(index, instance);
            }

            return instance;
        }

        T[] GetManyCached<T>(Dictionary<int, Component[]> storage, Func<T[]> getsMethod) where T : Component
        {
            var index = GetInfo<T>.Index;

            if (allocationEnabled)
            {
                storage ??= new Dictionary<int, Component[]>(16);

                if (storage.TryGetValue(index, out var components))
                {
                    return (T[]) components;
                }
            }

            var instances = getsMethod?.Invoke();

            if (allocationEnabled && instances != null)
            {
                storage.Add(index, instances);
            }

            return instances;
        }
    }
}