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
    public class TerrainTool : EditorWindow
    {
        [UnityEditor.MenuItem("Tools/" + nameof(Neeto.TerrainTool), priority = MENU.Top)]
        public static TerrainTool Open()
        {
            var window = GetWindow<TerrainTool>(false, "Terrain Replacer", true);
            window.Show();
            return window;
        }

        Terrain sceneTerrain;
        TerrainData sceneTerrainData;
        TerrainData selectedTerrainData;

        void OnEnable()
        {
            sceneTerrain = GameObject.FindObjectOfType<Terrain>();
            sceneTerrainData = sceneTerrain.terrainData;
        }

        void OnGUI()
        {
            EditorGUILayout.ObjectField(sceneTerrain, typeof(Terrain), true);
            var cmd = "Clone Terrain";
            if (GUILayout.Button(cmd))
            {
                var obj = NGUI.CreateUniqueAsset(sceneTerrain.terrainData);
                ApplyTerrain(obj);
            }
        }

        void ApplyTerrain(TerrainData terrainData)
        {
            var collider = sceneTerrain.GetComponent<TerrainCollider>();
            var oldData = sceneTerrain.terrainData;

            Undo.undoRedoPerformed += () =>
            {
                sceneTerrain.terrainData = oldData;
                collider.terrainData = oldData;
                sceneTerrainData = oldData;
            };

            sceneTerrain.terrainData = terrainData;
            collider.terrainData = terrainData;
            sceneTerrainData = terrainData;

            EditorUtility.SetDirty(sceneTerrain);
            EditorUtility.SetDirty(collider);

            OnEnable();
        }
    }
#endif
}