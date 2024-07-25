using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Generic pool. Caches specified component allowing not to use GetComponent<> after each call. Can not be added into the PoolManager.
    /// To use just create new instance.
    /// </summary>
    /// <typeparam name="T">Component to cache.</typeparam>
    [System.Serializable]
    public class PoolGeneric<T> : Pool where T : Component
    {
        public List<T> pooledComponents = new();
        public List<List<T>> multiPooledComponents = new();

        public delegate void TCallback(T value);

        public void ForEach(TCallback callback)
        {
            for(var i = 0; i < pooledComponents.Count; i++)
            {
                callback(pooledComponents[i]);
            }
        }

        public PoolGeneric(PoolSettings settings) : base(settings)
        {

        }

        protected override void InitGenericSingleObject(GameObject prefab)
        {
            var component = prefab.GetComponent<T>();

            if (component != null)
            {
                pooledComponents.Add(component);
            }
            else
            {
                Debug.LogError("There's no attached component of type: " + typeof(T).ToString() + " on prefab at pool called: " + Name);
            }
        }

        protected override void InitGenericMultiObject(int poolIndex, GameObject prefab)
        {
            if (poolIndex >= multiPooledComponents.Count)
            {
                for (var i = 0; i < poolIndex - multiPooledComponents.Count + 1; i++)
                {
                    multiPooledComponents.Add(new List<T>());
                }
            }

            multiPooledComponents[poolIndex].Add(prefab.GetComponent<T>());
        }

        /// <summary>
        /// Returns reference to pooled object if it's currently available.
        /// </summary>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
        public T GetPooledComponent(bool activateObject = true)
        {
            return GetPooledComponent(true, activateObject, false, Vector3.zero);
        }

        public T[] GetPooledComponents(int amount, bool activateObject = true)
        {
            return GetPooledComponents(amount, true, activateObject, false, Vector3.zero);
        }

        /// <summary>
        /// Returns reference to pooled object if it's currently available.
        /// </summary>
        /// <param name="position">Sets object to specified position.</param>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
        public T GetPooledComponent(Vector3 position, bool activateObject = true)
        {
            return GetPooledComponent(true, activateObject, true, position);
        }


        /// <summary>
        /// Rerurns reference to pooled object if it's currently available.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public T GetPooledComponent(PooledObjectSettings settings)
        {
            if (type == PoolType.Single)
            {
                return GetPooledComponentSingleType(settings);
            }
            else
            {
                return GetPooledComponentMultiType(settings, -1);
            }
        }

        /// <summary>
        /// Internal override of GetPooledObject and GetHierarchyPooledObject methods.
        /// </summary>
        /// <param name="checkTypeActiveSelf">Which type of checking object's activation state is used: active self or active in hierarchy.</param>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <param name="position">Sets object to specified position.</param>
        /// <returns></returns>
        T GetPooledComponent(bool checkTypeActiveSelf, bool activateObject, bool setPosition, Vector3 position)
        {
            var settings = new PooledObjectSettings(activateObject, !checkTypeActiveSelf);

            if (setPosition)
            {
                settings = settings.SetPosition(position);
            }

            if (type == PoolType.Single)
            {
                return GetPooledComponentSingleType(settings);
            }
            else
            {
                return GetPooledComponentMultiType(settings, -1);
            }
        }

        T[] GetPooledComponents(int amount, bool checkTypeActiveSelf, bool activateObject, bool setPosition, Vector3 position)
        {
            var settings = new PooledObjectSettings(activateObject, !checkTypeActiveSelf);

            if (setPosition)
            {
                settings = settings.SetPosition(position);
            }

            if (type == PoolType.Single)
            {
                return GetPooledComponentsSingleType(amount, settings);
            }
            else
            {
                // Change Later
                //return GetPooledComponentMultiType(settings, -1);
                return GetPooledComponentsSingleType(amount, settings);
            }
        }

        /// <summary>
        /// Internal implementation of GetPooledObject and GetHierarchyPooledObject methods for Single type pool.
        /// </summary>
        /// <param name="checkTypeActiveSelf">Which type of checking object's activation state is used: active self or active in hierarchy.</param>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <param name="position">Sets object to specified position.</param>
        /// <returns></returns>
        T GetPooledComponentSingleType(PooledObjectSettings settings)
        {
            if (!isInit)
                InitializeAsSingleTypePool();

            for (var i = 0; i < PooledObjects.Count; i++)
            {
                var pooledObject = PooledObjects[i];

                if(pooledObject == null)
                {
                    // Creating a new object

                    Debug.LogWarning("Destroyed pool object located: " + singlePoolPrefab.name);

                    var newObject = PoolManager.SpawnObject(singlePoolPrefab, objectsContainer);

                    newObject.name += " " + PoolManager.SpawnedObjectsAmount;
                    newObject.SetActive(false);

                    PooledObjects[i] = newObject;

                    InitGenericSingleObject(newObject);

                    pooledComponents[i] = newObject.GetComponent<T>();
                }

                if (settings.UseActiveOnHierarchy ? !PooledObjects[i].activeInHierarchy : !PooledObjects[i].activeSelf)
                {
                    SetupPooledObject(PooledObjects[i], settings);
                    return pooledComponents[i];
                }
            }

            if (autoSizeIncrement)
            {
                var newObject = AddObjectToPoolSingleType(" e");
                SetupPooledObject(newObject, settings);

                return pooledComponents[pooledComponents.Count - 1];
            }

            return null;
        }

        T[] GetPooledComponentsSingleType(int amount, PooledObjectSettings settings)
        {
            if (!isInit)
                InitializeAsSingleTypePool();

            var result = new T[amount];

            var counter = 0;

            for (var i = 0; i < PooledObjects.Count; i++)
            {
                var obj = PooledObjects[i];
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);

                    result[counter] = pooledComponents[i];

                    counter++;

                    if(counter == amount)
                    {
                        return result;
                    }
                }
            }

            for(var i = counter; i < amount; i++)
            {
                var index = pooledComponents.Count;

                var newObject = AddObjectToPoolSingleType(" e");

                newObject.SetActive(true);

                result[i] = pooledComponents[index];
            }

            return result;
        }

        /// <summary>
        /// Internal implementation of GetPooledObject and GetHierarchyPooledObject methods for Multi type pool.
        /// </summary>
        /// <param name="checkTypeActiveSelf">Which type of checking object's activation state is used: active self or active in hierarchy.</param>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <param name="position">Sets object to specified position.</param>
        /// <returns></returns>
        T GetPooledComponentMultiType(PooledObjectSettings settings, int poolIndex)
        {
            if (!isInit)
                InitializeAsMultiTypePool();

            var chosenPoolIndex = 0;

            if (poolIndex != -1)
            {
                chosenPoolIndex = poolIndex;
            }
            else
            {
                var randomPoolIndex = 0;
                var randomValueWasInRange = false;
                var randomValue = UnityEngine.Random.Range(1, 101);
                var currentValue = 0;

                for (var i = 0; i < multiPoolPrefabsList.Count; i++)
                {
                    currentValue += multiPoolPrefabsList[i].weight;

                    if (randomValue <= currentValue)
                    {
                        randomPoolIndex = i;
                        randomValueWasInRange = true;
                        break;
                    }
                }

                if (!randomValueWasInRange)
                {
                    Debug.LogError("[Pool Manager] Random value(" + randomValue + ") is out of weights sum range at pool: \"" + name + "\"");
                }

                chosenPoolIndex = randomPoolIndex;
            }

            var objectsList = MultiPooledObjects[chosenPoolIndex];

            for (var i = 0; i < objectsList.Count; i++)
            {
                if (settings.UseActiveOnHierarchy ? !objectsList[i].activeInHierarchy : !objectsList[i].activeSelf)
                {
                    SetupPooledObject(objectsList[i], settings);
                    return multiPooledComponents[chosenPoolIndex][i];
                }
            }

            if (autoSizeIncrement)
            {
                var newObject = AddObjectToPoolMultiType(chosenPoolIndex, " e");
                SetupPooledObject(newObject, settings);

                return multiPooledComponents[chosenPoolIndex][multiPooledComponents[chosenPoolIndex].Count - 1];
            }

            return null;
        }

        protected override void OnPoolCleared()
        {
            if (type == PoolType.Single)
            {
                pooledComponents.Clear();
            }
            else
            {
                for (var i = 0; i < multiPooledComponents.Count; i++)
                {
                    multiPooledComponents[i].Clear();
                }

                multiPooledComponents.Clear();
            }
        }
    }
}

// -----------------
// Pool Manager v 1.6.5
// -----------------