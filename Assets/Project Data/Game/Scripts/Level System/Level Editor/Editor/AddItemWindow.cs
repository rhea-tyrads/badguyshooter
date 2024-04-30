using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace Watermelon.LevelSystem
{
    public class AddItemWindow : EditorWindow
    {
        private static EditorWindow window;
        private const int PREVIEW_SIZE = 128;
        private const string ITEMS_PROPERTY_PATH = "items";
        private const string PREFAB_PROPERTY_PATH = "prefab";
        private const string TYPE_PROPERTY_PATH = "type";
        private const string HASH_PROPERTY_PATH = "hash";
        private LevelsDatabase levelsDatabase;
        private string[] worldSelection;
        private string[] levelItemTypeSelection;
        private int selectedWorld;
        private int selectedType;
        private static GameObject objectRef;
        private Rect globalRect;
        private Rect layoutRect;
        private Rect textureRect;
        private ValidataionStatus status;
        private string validationMessage;

        [MenuItem("Assets/Add into Level Editor", priority = 100)]
        public static void OpenWindow()
        {
            objectRef = Selection.activeObject as GameObject;
            window = EditorWindow.GetWindow(typeof(AddItemWindow));
            window.titleContent = new GUIContent("Adding new level item");
            window.maxSize = new Vector2(300, 250);
            window.minSize = new Vector2(300, 250);
            window.Show();
        }

        [MenuItem("Assets/Add into Level Editor", true, 0)]
        public static bool ValidateOpenWindow()
        {
            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        private void OnEnable()
        {
            levelsDatabase = EditorUtils.GetAsset<LevelsDatabase>();
            worldSelection = new string[levelsDatabase.Worlds.Length];

            for (int i = 0; i < worldSelection.Length; i++)
            {
                worldSelection[i] = levelsDatabase.Worlds[i].WorldType.ToString();
            }

            levelItemTypeSelection = Enum.GetNames(typeof(LevelItemType));

            textureRect = new Rect();
            selectedWorld = -1;
            selectedType = -1;
        }

        private void OnGUI()
        {
            globalRect = EditorGUILayout.BeginVertical();
            layoutRect = EditorGUILayout.BeginVertical();
            GUILayout.Space(PREVIEW_SIZE);
            EditorGUILayout.EndVertical();

            textureRect.Set(layoutRect.x + layoutRect.width/2f - PREVIEW_SIZE/2f, layoutRect.y, PREVIEW_SIZE, PREVIEW_SIZE);
            GUI.DrawTexture(textureRect, AssetPreview.GetAssetPreview(objectRef));


            selectedWorld = EditorGUILayout.Popup("World:",selectedWorld, worldSelection);
            selectedType = EditorGUILayout.Popup("Type:", selectedType, levelItemTypeSelection);
            validationMessage = GetValidationMessage();

            if(status == ValidataionStatus.PrefabInvalid)
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Error);
            }
            else if(status == ValidataionStatus.FieldsNotSet)
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Info);
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel", WatermelonEditor.Styles.button_03))
            {
                Close();
            }

            EditorGUI.BeginDisabledGroup(status != ValidataionStatus.PrefabValid);

            if(GUILayout.Button("Add", WatermelonEditor.Styles.button_02))
            {
                AddNewElement();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void AddNewElement()
        {
            SerializedObject worldObject = new SerializedObject(levelsDatabase.Worlds[selectedWorld]);
            SerializedProperty itemsProperty = worldObject.FindProperty(ITEMS_PROPERTY_PATH);


            int hash = TimeUtils.GetCurrentUnixTimestamp().GetHashCode();
            bool unique = true;

            do
            {
                if (!unique)
                {
                    hash = (TimeUtils.GetCurrentUnixTimestamp() + UnityEngine.Random.Range(1, 1000)).GetHashCode();
                }

                for (int i = 0; unique && (i < itemsProperty.arraySize); i++)
                {
                    if (itemsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(HASH_PROPERTY_PATH).intValue == hash)
                    {
                        unique = false;
                    }
                }

            } while (!unique);

            itemsProperty.arraySize++;

            SerializedProperty newElement = itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1);
            newElement.FindPropertyRelative(HASH_PROPERTY_PATH).intValue = hash;
            newElement.FindPropertyRelative(TYPE_PROPERTY_PATH).intValue = selectedType;
            newElement.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue = objectRef;
            worldObject.ApplyModifiedProperties();
            Close();
        }

        private string GetValidationMessage()
        {
            if((selectedWorld == -1) || (selectedType == -1))
            {
                status = ValidataionStatus.FieldsNotSet;
                return "Please set values to World and Type popups.";
            }


            status = ValidataionStatus.PrefabInvalid;

            if (objectRef.GetComponent<Collider>() == null)
            {
                return "Prefab doesn't have a Collider.";
            }

            if (selectedType == (int)LevelItemType.Obstacle)
            {
                if (objectRef.GetComponent<NavMeshObstacle>() == null)
                {
                    return "Prefab doesn't have a NavMeshObstacle.";
                }

                if (objectRef.GetComponent<NavMeshModifier>() == null)
                {
                    return "Prefab doesn't have a NavMeshModifier.";
                }

                if (objectRef.layer != LayerMask.NameToLayer("Obstacle"))
                {
                    return "Prefab assigned to incorrect layer. Obstacle is the only correct layer for Obstacle type items..";
                }

            }
            else if(selectedType == (int)LevelItemType.Environment)
            {
                if (!((objectRef.layer == LayerMask.NameToLayer("Obstacle")) || (objectRef.layer == LayerMask.NameToLayer("Ground"))))
                {
                    return "Prefab assigned to incorrect layer. Obstacle or Ground can be assigned as correct layers for Environment type items.";
                }
            }

            status = ValidataionStatus.PrefabValid;
            return "Prefab passed validation.";
        }

        private enum ValidataionStatus
        {
            PrefabInvalid,
            FieldsNotSet,
            PrefabValid
        }
    }
}
