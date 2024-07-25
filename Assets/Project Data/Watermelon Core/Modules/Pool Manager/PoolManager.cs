#pragma warning disable 0414

using System;
using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
 
    public class PoolManager : MonoBehaviour
    {
        static PoolManager instance;
 
        [SerializeField] List<Pool> poolsList = new();
 
        Dictionary<string, Pool> poolsDictionary;

        int spawnedObjectAmount = 0;
 
        public static int SpawnedObjectsAmount => instance.spawnedObjectAmount;

        static Transform objectsContainer;
        public static Transform ObjectsContainerTransform => objectsContainer;

        void Awake()
        {
            InitSingleton(this);
        }

 
        static void InitSingleton(PoolManager poolManager = null)
        {
            if (instance != null) return;

            if(poolManager == null)
                poolManager = FindObjectOfType<PoolManager>();

            if (poolManager != null)
            {
                // Save instance
                instance = poolManager;

#if UNITY_EDITOR
                var containerObject = new GameObject("[POOL OBJECTS]");
                objectsContainer = containerObject.transform;
                objectsContainer.ResetGlobal();
#endif

                poolManager.poolsDictionary = new Dictionary<string, Pool>();

                foreach (var pool in poolManager.poolsList)
                {
                    poolManager.poolsDictionary.Add(pool.Name, pool);
                    pool.Initialize();
                }

                return;
            }

            Debug.LogError("[PoolManager]: Please, add PoolManager behaviour at scene.");
        }

        public static void Unload()
        {
            var poolManager = instance;
            if (poolManager == null) return;
            foreach (var pool in poolManager.poolsList)
            {
                pool.ReturnToPoolEverything(true);
            }
        }

        public static GameObject SpawnObject(GameObject prefab, Transform parrent)
        {
#if UNITY_EDITOR
            if (parrent == null)
                parrent = ObjectsContainerTransform;
#endif

            instance.spawnedObjectAmount++;

            return Instantiate(prefab, parrent);
        }

        public static Pool GetPoolByName(string poolName)
        {
            InitSingleton();

            if (instance.poolsDictionary.ContainsKey(poolName))
                return instance.poolsDictionary[poolName];

            Debug.LogError("[PoolManager] Not found pool with name: '" + poolName + "'");

            return null;
        }

        public static PoolGeneric<T> GetPoolByName<T>(string poolName) where T : Component
        {
            InitSingleton();

            if (instance.poolsDictionary.ContainsKey(poolName))
            {
                var unboxedPool = instance.poolsDictionary[poolName];

                try
                {
                    return unboxedPool as PoolGeneric<T>;
                }
                catch (Exception)
                {
                    Debug.Log($"[PoolManager] Could not convert pool with name {poolName} to {typeof(PoolGeneric<T>)}");

                    return null;
                }
            }

            Debug.LogError("[PoolManager] Not found generic pool with name: '" + poolName + "'");

            return null;
        }
 
        public static Pool AddPool(PoolSettings poolBuilder)
        {
            InitSingleton();

            if (instance.poolsDictionary.ContainsKey(poolBuilder.name))
            {
                Debug.LogError("[Pool manager] Adding a new pool failed. Name \"" + poolBuilder.name + "\" already exists.");
                return GetPoolByName(poolBuilder.name);
            }

            var newPool = new Pool(poolBuilder);
            instance.poolsDictionary.Add(newPool.Name, newPool);
            instance.poolsList.Add(newPool);

            newPool.Initialize();

            return newPool;
        }

        public static PoolGeneric<T> AddPool<T>(PoolSettings poolBuilder) where T : Component
        {
            InitSingleton();

            if (instance.poolsDictionary.ContainsKey(poolBuilder.name))
            {
                Debug.LogError("[Pool manager] Adding a new pool failed. Name \"" + poolBuilder.name + "\" already exists.");
                return GetPoolByName<T>(poolBuilder.name);
            }

            var poolGeneric = new PoolGeneric<T>(poolBuilder);
            instance.poolsDictionary.Add(poolGeneric.Name, poolGeneric);
            instance.poolsList.Add(poolGeneric);

            poolGeneric.Initialize();

            return poolGeneric;
        }

        public static void AddPool(Pool pool)
        {
            InitSingleton();

            if (instance.poolsDictionary.ContainsKey(pool.Name))
            {
                Debug.LogError("[Pool manager] Adding a new pool failed. Name \"" + pool.Name + "\" already exists.");

                return;
            }

            instance.poolsDictionary.Add(pool.Name, pool);
            instance.poolsList.Add(pool);

            pool.Initialize();
        }

        public static void DestroyPool(Pool pool)
        {
            pool.Clear();

            instance.poolsDictionary.Remove(pool.Name);
            instance.poolsList.Remove(pool);
        }

        public static bool PoolExists(string name) 
            => instance  && instance.poolsDictionary.ContainsKey(name);

        public static void RemoveAllNullObjects()
        {
            foreach(var poolKeyValue in instance.poolsDictionary)
            {
                poolKeyValue.Value.DeleteAllNullRefsInSpawnedObjects();
            }
        }

        // editor methods

        bool IsAllPrefabsAssignedAtPool(int poolIndex) 
            => poolsList == null || poolIndex >= poolsList.Count || poolsList[poolIndex].IsAllPrefabsAssigned();

        void RecalculateWeightsAtPool(int poolIndex)
        {
            poolsList[poolIndex].RecalculateWeights();
        }
    }
}

 
