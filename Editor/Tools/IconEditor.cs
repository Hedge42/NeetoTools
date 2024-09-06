using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Neeto
{

    public class IconEditor : EditorWindow
    {
        private Texture2D iconTexture;
        public List<Object> objectsToIconize = new List<Object>();
        private Vector2 leftScroll;
        private Vector2 rightScroll;

        private SerializedObject so;

        [MenuItem("Window/Icon Applier")]
        public static void ShowWindow()
        {
            GetWindow<IconEditor>("Icon Applier");
        }

        private void OnEnable()
        {
            so = new SerializedObject(this);
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            DrawLeftPanel();
            //DrawRightPanel();

            EditorGUILayout.EndHorizontal();

            DrawBottomPanel();
        }

        void DrawLeftPanel()
        {
            leftScroll = EditorGUILayout.BeginScrollView(leftScroll);
            var prop = so.FindProperty(nameof(objectsToIconize));
            EditorGUILayout.PropertyField(prop);
            so.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f);
            GUI.Box(dropArea, "Drag Assets Here");

            rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

            // Drag-and-drop logic
            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(Event.current.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            string assetPath = AssetDatabase.GetAssetPath(draggedObject);
                            if (!string.IsNullOrEmpty(assetPath) && !objectsToIconize.Contains(draggedObject))
                            {
                                objectsToIconize.Add(draggedObject);
                            }
                        }
                    }
                    Event.current.Use();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void DrawBottomPanel()
        {
            EditorGUILayout.BeginHorizontal();
            iconTexture = (Texture2D)EditorGUILayout.ObjectField("Icon Texture", iconTexture, typeof(Texture2D), false);

            GUI.enabled = iconTexture != null && objectsToIconize.Count > 0;

            if (GUILayout.Button("Apply Icon"))
            {
                ApplyIconToAssets();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        void ApplyIconToAssets()
        {
            foreach (Object obj in objectsToIconize)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    Undo.RecordObject(obj, "Apply Icon");
                    EditorGUIUtility.SetIconForObject(obj, iconTexture);
                    EditorUtility.SetDirty(obj);
                }
            }
            AssetDatabase.SaveAssets();
        }
    }

}