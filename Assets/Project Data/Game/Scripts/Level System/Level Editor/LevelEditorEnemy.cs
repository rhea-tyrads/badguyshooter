#pragma warning disable 649
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [ExecuteInEditMode]
    public class LevelEditorEnemy : MonoBehaviour
    {
#if UNITY_EDITOR
        public EnemyType type;
        public bool isElite;
        public List<Transform> pathPoints;
        public Transform pathPointsContainer;

        //Gizmo
        const int LINE_HEIGHT = 5;
        Color enemyColor;
        Color defaultColor;
        Color goldColor;
        Material enemyMaterial;
        StartPointHandles startPointHandles;
        bool isStartPointHandlesInited;


        public void Awake()
        {
            pathPoints = new List<Transform>();
            enemyColor = Random.ColorHSV(0f, 1, 0.75f, 1, 1f, 1, 1, 1);
            enemyMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            enemyMaterial.color = enemyColor;
            goldColor = new Color(1, 204 / 255f, 0);
            isStartPointHandlesInited = false;
        }

        public void Update()
        {
            for (var i = 0; i < pathPointsContainer.childCount; i++)
            {
                if (!pathPoints.Contains(pathPointsContainer.GetChild(i)))
                {
                    pathPoints.Add(pathPointsContainer.GetChild(i));
                }
            }

            for (var i = pathPoints.Count - 1; i >= 0; i--)
            {
                if(pathPoints[i] == null)
                {
                    pathPoints.RemoveAt(i);
                }
            }

            if(isElite && (!isStartPointHandlesInited))
            {
                startPointHandles =  gameObject.AddComponent<StartPointHandles>();
                startPointHandles.diskRadius = 0.7f;
                startPointHandles.thickness = 7f;
                startPointHandles.diskColor = goldColor;
                startPointHandles.displayText = false;
                isStartPointHandlesInited = true;
            }
            else if(!isElite && isStartPointHandlesInited)
            {
                DestroyImmediate(startPointHandles);
                isStartPointHandlesInited = false;
            }

        }

        [Button]
        public void AddPathPoint()
        {
            GameObject sphere;

            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(pathPointsContainer);
            sphere.transform.position = pathPointsContainer.transform.parent.transform.position.SetY(0) + Vector3.back;
            sphere.transform.localScale = Vector3.one * 0.78125f;
            pathPoints.Add(sphere.transform);

            sphere.GetComponent<MeshRenderer>().sharedMaterial = enemyMaterial;
            Selection.activeGameObject = sphere;
        }

        [Button]
        public void ApplyMaterialToPathPoints()
        {
            MeshRenderer renderer;

            for (var i = 0; i < pathPoints.Count; i++)
            {
                renderer = pathPoints[i].GetComponent<MeshRenderer>();
                renderer.sharedMaterial = enemyMaterial;
            }
        }

        void DrawLine(Vector3 tempLineStart, Vector3 tempLineEnd)
        {
            var offset = Vector3.zero.AddToY(0.01f);

            for (var i = 0; i < LINE_HEIGHT; i++)
            {
                Gizmos.DrawLine(tempLineStart + i * offset, tempLineEnd + i * offset);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = enemyColor;

            Gizmos.DrawSphere(transform.position + Vector3.up, 0.3125f);

            for (var i = 0; i < pathPoints.Count - 1; i++)
            {
                DrawLine(pathPoints[i].transform.position, pathPoints[i + 1].transform.position);
            }


            Gizmos.color = defaultColor;
        }

        public Vector3[] GetPathPoints()
        {
            var result = new Vector3[pathPoints.Count];

            for (var i = 0; i < pathPoints.Count; i++)
            {
                result[i] = pathPoints[i].localPosition + pathPointsContainer.transform.parent.localPosition;
            }

            return result;
        }

#endif
    }
}