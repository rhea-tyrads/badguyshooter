#pragma warning disable 649

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public class EditorSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        static EditorSceneController instance;
        public static EditorSceneController Instance { get => instance; }

        [SerializeField] GameObject container;
        [SerializeField] GameObject roomCustomObjectsContainer;
        [SerializeField] GameObject worldCustomObjectsContainer;
        [SerializeField] Vector3 spawnPoint;
        [SerializeField] Vector3 exitPoint;
        [SerializeField] float spawnPointSphereSize;
        [SerializeField] float exitPointSphereSize;
        [SerializeField] Color spawnPointColor;
        [SerializeField] Color exitPointColor;
        Color backupColor;
        bool showGizmo;

        public GameObject Container { set => container = value; }
        public Vector3 SpawnPoint { get => spawnPoint; set => spawnPoint = value; }
        public Vector3 ExitPoint { get => exitPoint; set => exitPoint = value; }
        public Color SpawnPointColor { get => spawnPointColor; set => spawnPointColor = value; }
        public Color ExitPointColor { get => exitPointColor; set => exitPointColor = value; }

        public EditorSceneController()
        {
            instance = this;
        }



        public void SpawnItem(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, int hash, bool selectSpawnedItem = false)
        {
            var gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(container.transform);

            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            var levelEditorItem = gameObject.AddComponent<LevelEditorItem>();
            levelEditorItem.hash = hash;

            if (selectSpawnedItem)
            {
                Selection.activeGameObject = gameObject;
            }
        }


        public void SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, EnemyType type, bool isElite, Vector3[] pathPoints)
        {
            var gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(container.transform);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            var levelEditorEnemy = gameObject.AddComponent<LevelEditorEnemy>();
            levelEditorEnemy.type = type;
            levelEditorEnemy.isElite = isElite;
            var pointsContainer = new GameObject("PathPointsContainer");
            pointsContainer.transform.SetParent(gameObject.transform);
            levelEditorEnemy.pathPointsContainer = pointsContainer.transform;
            pointsContainer.transform.localPosition = Vector3.zero;

            GameObject sphere;

            for (var i = 0; i < pathPoints.Length; i++)
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(levelEditorEnemy.pathPointsContainer);
                sphere.transform.localPosition = pathPoints[i] - gameObject.transform.localPosition;
                sphere.transform.localScale = Vector3.one * 0.78125f;
                levelEditorEnemy.pathPoints.Add(sphere.transform);
            }

            levelEditorEnemy.ApplyMaterialToPathPoints();
            Selection.activeGameObject = gameObject;
        }

        public void SpawnExitPoint(GameObject prefab, Vector3 position)
        {
            var gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(container.transform);
            gameObject.transform.localPosition = position;

            var exitPoint = gameObject.AddComponent<LevelEditorExitPoint>();
        }

        public void SpawnChest(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, LevelChestType type)
        {
            var gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(container.transform);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            var levelEditorChest = gameObject.AddComponent<LevelEditorChest>();
            levelEditorChest.type = type;
            Selection.activeGameObject = gameObject;
        }

        public void UpdateContainerLabel(int index)
        {
            if(index == -1)
            {
                container.name = "Container";
                showGizmo = false;
            }
            else
            {
                container.name = $"Container( Room #{index + 1})";
                showGizmo = true;
            }
        }
        public ItemEntityData[] CollectItemsFromRoom()
        {
            var editorData = container.GetComponentsInChildren<LevelEditorItem>();
            var result = new ItemEntityData[editorData.Length];

            for (var i = 0; i < editorData.Length; i++)
            {
                result[i] = new ItemEntityData(editorData[i].hash, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].transform.localScale);
            }

            return result;
        }

        public EnemyEntityData[] CollectEnemiesFromRoom()
        {
            var editorData = container.GetComponentsInChildren<LevelEditorEnemy>();
            var result = new EnemyEntityData[editorData.Length];

            for (var i = 0; i < editorData.Length; i++)
            {
                result[i] = new EnemyEntityData(editorData[i].type, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].transform.localScale, editorData[i].isElite,editorData[i].GetPathPoints());
            }

            return result;
        }

        public bool CollectExitPointFromRoom(out Vector3 position)
        {
            var editorData = container.GetComponentInChildren<LevelEditorExitPoint>();

            if(editorData == null)
            {
                position = Vector3.zero;

                return false;
            }
            else
            {
                position = editorData.transform.localPosition;

                return true;
            }
        }

        public List<LevelEditorChest> CollectChestFromRoom()
        {
            var result = new List<LevelEditorChest>();
            container.GetComponentsInChildren(result);
            return result;
        }

        public void SpawnRoomCustomObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(roomCustomObjectsContainer.transform);

            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
        }

        public void SpawnWorldCustomObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(worldCustomObjectsContainer.transform);

            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
        }

        public List<CustomObjectData> CollectRoomCustomObjects()
        {
            var result = new List<CustomObjectData>();
            Transform temp;

            for (var i = 0; i < roomCustomObjectsContainer.transform.childCount; i++)
            {
                temp = roomCustomObjectsContainer.transform.GetChild(i);

                if (PrefabUtility.IsPartOfAnyPrefab(temp))
                {
                    result.Add(new CustomObjectData(PrefabUtility.GetCorrespondingObjectFromSource(temp.gameObject), temp.localPosition, temp.localRotation, temp.localScale));
                }

            }

            return result;
        }

        public List<CustomObjectData> CollectWorldCustomObjects()
        {
            var result = new List<CustomObjectData>();
            Transform temp;

            for (var i = 0; i < worldCustomObjectsContainer.transform.childCount; i++)
            {
                temp = worldCustomObjectsContainer.transform.GetChild(i);

                if (PrefabUtility.IsPartOfAnyPrefab(temp))
                {
                    result.Add(new CustomObjectData(PrefabUtility.GetCorrespondingObjectFromSource(temp.gameObject), temp.localPosition, temp.localRotation, temp.localScale));
                }

            }

            return result;
        }


        public void Clear()
        {
            for (var i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }

            container.transform.ResetGlobal();
        }

        public void ClearRoomCustomObjectsContainer()
        {
            for (var i = roomCustomObjectsContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(roomCustomObjectsContainer.transform.GetChild(i).gameObject);
            }

            roomCustomObjectsContainer.transform.ResetGlobal();
        }

        public void ClearWorldCustomObjectsContainer()
        {
            for (var i = worldCustomObjectsContainer.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(worldCustomObjectsContainer.transform.GetChild(i).gameObject);
            }

            worldCustomObjectsContainer.transform.ResetGlobal();
        }

        public void OnDrawGizmos()
        {
            if(showGizmo)
            {
                backupColor = Gizmos.color;

                Gizmos.color = spawnPointColor;
                Gizmos.DrawWireSphere(container.transform.position + spawnPoint, spawnPointSphereSize);

                Gizmos.color = exitPointColor;
                Gizmos.DrawWireSphere(container.transform.position + exitPoint, exitPointSphereSize);

                Gizmos.color = backupColor;
            }
            
        }
#endif
    }
}