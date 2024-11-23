using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
#if UNITY_EDITOR
    public class TerrainGadget : EditorWindow
    {
        [UnityEditor.MenuItem(Menu.Open + nameof(Neeto.TerrainGadget), priority = Menu.Top)]
        public static TerrainGadget Open()
        {
            var window = GetWindow<TerrainGadget>(false, "Terrain Replacer", true);
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