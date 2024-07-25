using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using Watermelon.SquadShooter;
using static UnityEngine.Object;

namespace Watermelon
{
    [Serializable]
    public class Pool
    {
        [SerializeField] protected string name;
        public string Name => name;
        [SerializeField] protected PoolType type;
        public PoolType Type => type;
        [SerializeField] protected GameObject singlePoolPrefab = null;
        public GameObject SinglePoolPrefab => singlePoolPrefab;
        [SerializeField] protected List<MultiPoolPrefab> multiPoolPrefabsList;
        public int MultiPoolPrefabsAmount => multiPoolPrefabsList.Count;
        [SerializeField] int size;
        public int Size => size;
        [SerializeField] protected bool autoSizeIncrement;
        public bool AutoSizeIncrement => autoSizeIncrement;
        [SerializeField] protected Transform objectsContainer = null;
        public Transform ObjectsContainer => objectsContainer;
        [SerializeField] bool isRuntimeCreated;
        [SerializeField] protected bool isInit;
        protected List<GameObject> PooledObjects = new();
        protected List<List<GameObject>> MultiPooledObjects = new();

#if UNITY_EDITOR
        /// <summary>
        /// Number of objects that where active at one time.
        /// </summary>
        protected int maxItemsUsedInOneTime = 0;
#endif

        public enum PoolType
        {
            Single = 0,
            Multi = 1,
        }

        [Serializable]
        public struct MultiPoolPrefab
        {
            public GameObject prefab;
            public int weight;
            public bool isWeightLocked;

            public MultiPoolPrefab(GameObject prefab, int weight, bool isWeightLocked)
            {
                this.prefab = prefab;
                this.weight = weight;
                this.isWeightLocked = isWeightLocked;
            }
        }

        public Pool(PoolSettings builder)
        {
            name = builder.name;
            type = builder.type;
            singlePoolPrefab = builder.singlePoolPrefab;
            multiPoolPrefabsList = builder.multiPoolPrefabsList;
            size = builder.size;
            autoSizeIncrement = builder.autoSizeIncrement;
            objectsContainer = builder.objectsContainer;
            isRuntimeCreated = !PoolManager.PoolExists(name);
            isInit = false;
        }


        public void Initialize()
        {
            if (isInit) return;

            if (type == PoolType.Single)
                InitializeAsSingleTypePool();
            else
                InitializeAsMultiTypePool();
        }

        protected void InitializeAsSingleTypePool()
        {
            PooledObjects = new List<GameObject>();

            if (singlePoolPrefab)
            {
                for (var i = 0; i < size; i++)
                    AddObjectToPoolSingleType(" ");

                isInit = true;
            }
            else
            {
                Debug.LogError("[PoolManager] There's no attached prefab at pool: \"" + name + "\"");
            }
        }


        protected void InitializeAsMultiTypePool()
        {
            MultiPooledObjects = new List<List<GameObject>>();

            for (var i = 0; i < multiPoolPrefabsList.Count; i++)
            {
                MultiPooledObjects.Add(new List<GameObject>());

                if (multiPoolPrefabsList[i].prefab)
                {
                    for (var j = 0; j < size; j++)
                        AddObjectToPoolMultiType(i, " ");

                    isInit = true;
                }
                else
                {
                    Debug.LogError("[PoolManager] There's not attached prefab at pool: \"" + name + "\"");
                }
            }
        }

        protected virtual void InitGenericSingleObject(GameObject prefab)
        {
        }

        protected virtual void InitGenericMultiObject(int poolIndex, GameObject prefab)
        {
        }

        protected virtual void OnPoolCleared()
        {
        }

        /// <summary>
        /// Returns reference to pooled object if it's currently available.
        /// </summary>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
        public GameObject Get(bool activateObject = true)
            => Get(true, activateObject, false, Vector3.zero);

        /// <summary>
        /// Returns reference to pooled object if it's currently available.
        /// </summary>
        /// <param name="position">Sets object to specified position.</param>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
        public GameObject Get(Vector3 position, bool activateObject = true)
            => Get(true, activateObject, true, position);

        /// <summary>
        /// Returns reference to pooled object if it's currently available.
        /// </summary>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
        public GameObject GetHierarchyPooledObject(bool activateObject = true)
            => Get(false, activateObject, false, Vector3.zero);

        /// <summary>
        /// Returns reference to pooled object if it's currently available.
        /// </summary>
        /// <param name="position">Sets object to specified position.</param>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
        public GameObject GetHierarchyPooledObject(Vector3 position, bool activateObject = true) =>
            Get(false, activateObject, true, position);

        Dictionary<Transform, PlayerBulletBehavior> playerBullets = new();

        public PlayerBulletBehavior GetPlayerBullet(PooledObjectSettings settings)
        {
            var obj = Get(settings);
            
            if (playerBullets.ContainsKey(obj.transform))
                return playerBullets[obj.transform];

            var bullet = obj.GetComponent<PlayerBulletBehavior>();
            playerBullets.Add(obj.transform, bullet);
            return bullet;
        }

        public GameObject Get(PooledObjectSettings settings) => type == PoolType.Single
            ? GetSingle(settings)
            : GetMulti(settings, -1);


        GameObject Get(bool checkTypeActiveSelf, bool activateObject, bool setPosition, Vector3 position)
        {
            var settings = new PooledObjectSettings(activateObject, !checkTypeActiveSelf);
            if (setPosition)
                settings = settings.SetPosition(position);
            return type == PoolType.Single
                ? GetSingle(settings)
                : GetMulti(settings, -1);
        }


        GameObject GetSingle(PooledObjectSettings settings)
        {
            if (!isInit)
                InitializeAsSingleTypePool();

            for (var i = 0; i < PooledObjects.Count; i++)
            {
                var obj = PooledObjects[i];

                if (!obj)
                {
                    var newObject = PoolManager.SpawnObject(singlePoolPrefab, objectsContainer);
                    newObject.name += " " + PoolManager.SpawnedObjectsAmount;
                    newObject.SetActive(false);
                    PooledObjects[i] = newObject;
                    InitGenericSingleObject(newObject);
                }

                if (settings.UseActiveOnHierarchy
                        ? PooledObjects[i].activeInHierarchy
                        : PooledObjects[i].activeSelf) continue;
                SetupPooledObject(PooledObjects[i], settings);
                return PooledObjects[i];
            }

            if (!autoSizeIncrement) return null;
            {
                var newObject = AddObjectToPoolSingleType(" e");
                SetupPooledObject(newObject, settings);

                return newObject;
            }
        }


        GameObject GetMulti(PooledObjectSettings settings, int poolIndex)
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
                var currentValue = 0;
                var totalWeight = 0;

                for (var i = 0; i < multiPoolPrefabsList.Count; i++)
                {
                    totalWeight += multiPoolPrefabsList[i].weight;
                }

                var randomValue = UnityEngine.Random.Range(1, totalWeight);
                for (var i = 0; i < multiPoolPrefabsList.Count; i++)
                {
                    currentValue += multiPoolPrefabsList[i].weight;
                    if (randomValue > currentValue) continue;

                    randomPoolIndex = i;
                    randomValueWasInRange = true;
                    break;
                }

                if (!randomValueWasInRange)
                {
                    Debug.LogError("[Pool Manager] Random value(" + randomValue +
                                   ") is out of weights sum range at pool: \"" + name + "\"");
                }

                chosenPoolIndex = randomPoolIndex;
            }

            var objectsList = MultiPooledObjects[chosenPoolIndex];

            foreach (var obj in objectsList.Where(obj =>
                         settings.UseActiveOnHierarchy ? !obj.activeInHierarchy : !obj.activeSelf))
            {
                SetupPooledObject(obj, settings);
                return obj;
            }

            if (!autoSizeIncrement) return null;
            var newObject = AddObjectToPoolMultiType(chosenPoolIndex, " e");
            SetupPooledObject(newObject, settings);

            return newObject;
        }

        protected void SetupPooledObject(GameObject gameObject, PooledObjectSettings settings)
        {
            var objectTransform = gameObject.transform;
            if (settings.ApplyParrent) objectTransform.SetParent(settings.Parrent);
            if (settings.ApplyPosition) objectTransform.position = settings.Position;
            if (settings.ApplyLocalPosition) objectTransform.localPosition = settings.LocalPosition;
            if (settings.ApplyEulerRotation) objectTransform.eulerAngles = settings.EulerRotation;
            if (settings.ApplyLocalEulerRotation) objectTransform.localEulerAngles = settings.LocalEulerRotation;
            if (settings.ApplyRotation) objectTransform.rotation = settings.Rotation;
            if (settings.ApplyLocalRotation) objectTransform.rotation = settings.LocalRotation;
            if (settings.ApplyLocalScale) objectTransform.localScale = settings.LocalScale;
            gameObject.SetActive(settings.Activate);
        }

        protected GameObject AddObjectToPoolSingleType(string nameAddition)
        {
            var newObject = PoolManager.SpawnObject(singlePoolPrefab, objectsContainer);
            newObject.name += nameAddition + PoolManager.SpawnedObjectsAmount;
            newObject.SetActive(false);
            PooledObjects.Add(newObject);
            InitGenericSingleObject(newObject);
            return newObject;
        }

        public void CreatePoolObjects(int count)
        {
            var sizeDifference = count - PooledObjects.Count;
            if (sizeDifference <= 0) return;
            for (var i = 0; i < sizeDifference; i++)
            {
                AddObjectToPoolSingleType(" ");
            }
        }

        protected GameObject AddObjectToPoolMultiType(int PoolIndex, string nameAddition)
        {
            var newObject = PoolManager.SpawnObject(multiPoolPrefabsList[PoolIndex].prefab, objectsContainer);
            newObject.name += nameAddition + PoolManager.SpawnedObjectsAmount;
            newObject.SetActive(false);
            MultiPooledObjects[PoolIndex].Add(newObject);
            InitGenericMultiObject(PoolIndex, newObject);
            return newObject;
        }

        public void ResetParrents()
        {
            if (type == PoolType.Single)
            {
                foreach (var obj in PooledObjects)
                {
                    obj.transform.SetParent(objectsContainer != null
                        ? objectsContainer
                        : PoolManager.ObjectsContainerTransform);
                }
            }
            else
            {
                foreach (var obj in MultiPooledObjects.SelectMany(list => list))
                {
                    obj.transform.SetParent(objectsContainer != null
                        ? objectsContainer
                        : PoolManager.ObjectsContainerTransform);
                }
            }
        }


        public void ReturnToPoolEverything(bool resetParrent = false)
        {
            if (type == PoolType.Single)
            {
                foreach (var obj in PooledObjects)
                {
                    if (resetParrent)
                    {
                        obj.transform.SetParent(objectsContainer
                            ? objectsContainer
                            : PoolManager.ObjectsContainerTransform);
                    }

                    obj.SetActive(false);
                }
            }
            else
            {
                foreach (var obj in MultiPooledObjects.SelectMany(list => list))
                {
                    if (resetParrent)
                    {
                        obj.transform.SetParent(objectsContainer
                            ? objectsContainer
                            : PoolManager.ObjectsContainerTransform);
                    }

                    obj.SetActive(false);
                }
            }
        }


        public void Clear()
        {
            if (type == PoolType.Single)
            {
                foreach (var obj in PooledObjects)
                    Destroy(obj);
                PooledObjects.Clear();
            }
            else
            {
                foreach (var list in MultiPooledObjects)
                {
                    foreach (var obj in list)
                        Destroy(obj);
                    list.Clear();
                }
            }

            OnPoolCleared();
        }


        public GameObject GetMultiByIndex(int index, PooledObjectSettings setting) => GetMulti(setting, index);

        public MultiPoolPrefab MultiPoolByIndex(int index) => multiPoolPrefabsList[index];

        /// <summary>
        /// Evenly distributes the weight between multi pooled objects, leaving locked weights as is.
        /// </summary>
        public void RecalculateWeights()
        {
            var oldPrefabsList = new List<MultiPoolPrefab>(multiPoolPrefabsList);
            multiPoolPrefabsList = new List<MultiPoolPrefab>();

            if (oldPrefabsList.Count <= 0) return;
            var totalUnlockedPoints = 100;
            var unlockedPrefabsAmount = oldPrefabsList.Count;

            for (var i = 0; i < oldPrefabsList.Count; i++)
            {
                if (!oldPrefabsList[i].isWeightLocked) continue;
                totalUnlockedPoints -= oldPrefabsList[i].weight;
                unlockedPrefabsAmount--;
            }

            if (unlockedPrefabsAmount > 0)
            {
                var averagePoints = totalUnlockedPoints / unlockedPrefabsAmount;
                var additionalPoints = totalUnlockedPoints - averagePoints * unlockedPrefabsAmount;

                for (var j = 0; j < oldPrefabsList.Count; j++)
                {
                    if (oldPrefabsList[j].isWeightLocked)
                    {
                        multiPoolPrefabsList.Add(oldPrefabsList[j]);
                    }
                    else
                    {
                        multiPoolPrefabsList.Add(new MultiPoolPrefab(oldPrefabsList[j].prefab,
                            averagePoints + (additionalPoints > 0 ? 1 : 0), false));
                        additionalPoints--;
                    }
                }
            }
            else
            {
                multiPoolPrefabsList = oldPrefabsList;
            }
        }

        public bool IsAllPrefabsAssigned()
        {
            if (type == PoolType.Single)
                return singlePoolPrefab != null;

            if (multiPoolPrefabsList.Count == 0)
                return false;

            for (var i = 0; i < multiPoolPrefabsList.Count; i++)
            {
                if (multiPoolPrefabsList[i].prefab) continue;
                return false;
            }

            return true;
        }

        public void DeleteAllNullRefsInSpawnedObjects()
        {
            for (var i = 0; i < PooledObjects.Count; i++)
            {
                if (PooledObjects[i] != null) continue;
                Debug.Log("Found null ref in pool: " + name);
                PooledObjects.RemoveAt(i);
                i--;
            }
        }
    }
}