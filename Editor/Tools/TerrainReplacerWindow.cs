using UnityEngine;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
#if UNITY_EDITOR
    public class TerrainReplacerWindow : EditorWindow
    {
        [MenuItem(MenuPath.Open + nameof(TerrainReplacerWindow), priority = Priority.Min)]
        public static TerrainReplacerWindow Open()
        {
            var window = GetWindow<TerrainReplacerWindow>(false, "Terrain Replacer", true);
            window.Show();
            return window;
        }

        Terrain sceneTerrain;
        IEnumerable<TerrainData> terrains;
        TerrainData sceneData;
        TerrainData selected;

        protected void OnEnable()
        {
            sceneTerrain = GameObject.FindObjectOfType<Terrain>();
            sceneData = sceneTerrain.terrainData;
        }

        Vector2 scrollPosition;
        protected void OnGUI()
        {
            EditorGUILayout.ObjectField(sceneTerrain, typeof(Terrain), true);
            var cmd = "Clone Terrain";
            if (GUILayout.Button(cmd))
            {

                var obj = NGUI.CreateUniqueAsset(sceneTerrain.terrainData);
                //Undo.RegisterCreatedObjectUndo(obj, undo);

                ApplyTerrain(obj);
            }

            //top.y += NGUI.fullLineHeight;
            //Neeto.SplitVertical(top, .5f, out var left, out var right);
            //if (GUI.Button(left, "Replace with"))
            //{
                //ApplyTerrain(selected);
            //}


            //selected = (TerrainData)EditorGUI.ObjectField(right, selected, typeof(TerrainData), false);

            //var height = fullLineHeight * terrains.Count();
            //scrollPosition = VerticalScrollGUI(scrollPosition, bottom, height, OnTerrainGUI);
        }

        void ApplyTerrain(TerrainData terrainData)
        {
            var collider = sceneTerrain.GetComponent<TerrainCollider>();
            //Undo.RecordObject(sceneTerrain, "Terrain");
            //Undo.RecordObject(collider, "Terrain");

            var oldData = sceneTerrain.terrainData;

            Undo.undoRedoPerformed += () =>
            {
                sceneTerrain.terrainData = oldData;
                collider.terrainData = oldData;
                sceneData = oldData;
            };

            sceneTerrain.terrainData = terrainData;
            collider.terrainData = terrainData;
            sceneData = terrainData;

            EditorUtility.SetDirty(sceneTerrain);
            EditorUtility.SetDirty(collider);

            OnEnable();
        }

        void OnSceneTerrainGUI() { }

        Texture2D clearPixel;
        void OnTerrainGUI(Rect rect)
        {
            rect.height = NGUI.lineHeight;
            var style = new GUIStyle(GUI.skin.box);
            //style.normal.background = Utils.CachedPixel(Color.clear, ref clearPixel);
            style.alignment = TextAnchor.MiddleLeft;
            style.wordWrap = false;
            foreach (var data in terrains)
            {
                var path = AssetDatabase.GetAssetPath(data);
                if (GUI.Button(rect, path, style))
                {
                    selected = data;
                }

                //GUI.Label(rect, path, EditorStyles.objectField);
                //EditorGUI.ObjectField(rect, data, typeof(TerrainData), false);

                rect.y += NGUI.fullLineHeight;
            }
        }
    }
#endif
}