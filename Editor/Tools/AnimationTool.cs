using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Neeto
{
    class AnimationTool : EditorWindow
    {
        [UnityEditor.MenuItem("Tools/" + nameof(AnimationTool), priority =MENU.Top)]
        static void Open() => GetWindow<AnimationTool>();

        Object[] importers;
        ModelImporterAnimationType selectedAnimationType = ModelImporterAnimationType.Human;
        bool loopTime, mirror, applyRotation, applyYPosition, applyXZPosition;
        RotationBasedUponOption rootRotationBasedUpon = RotationBasedUponOption.Original;
        PositionYBasedUponOption rootYBasedUpon = PositionYBasedUponOption.Original;
        PositionXZBasedUponOption rootXZBasedUpon = PositionXZBasedUponOption.Original;
        Vector2 scrollPosition;

        enum RotationBasedUponOption { Original, BodyOrientation }
        enum PositionYBasedUponOption { Original, Feet, CenterOfMass }
        enum PositionXZBasedUponOption { Original, CenterOfMass }

        void OnEnable() => Selection.selectionChanged += OnSelectionChanged;
        void OnDisable() => Selection.selectionChanged -= OnSelectionChanged;

        void OnSelectionChanged()
        {
            importers = Selection.objects.Where(IsModelWithAnimations).ToArray();
            Repaint();
        }

        bool IsModelWithAnimations(Object obj) => AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(obj)).OfType<AnimationClip>().Any();

        void OnGUI()
        {
            if (importers == null)
                OnSelectionChanged();

            var headerColor = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = Color.cyan.WithH(.45f) } };
            var dropdownColor = new GUIStyle(EditorStyles.popup) { normal = { textColor = Color.white } };

            

            EditorGUILayout.LabelField("Animation Settings", headerColor);

            selectedAnimationType = (ModelImporterAnimationType)EditorGUILayout.EnumPopup("Animation Type", selectedAnimationType);
            loopTime = EditorGUILayout.ToggleLeft("Loop Time", loopTime);
            mirror = EditorGUILayout.ToggleLeft("Mirror", mirror);

            applyRotation = EditorGUILayout.ToggleLeft("Bake Root Transform Rotation", applyRotation);
            EditorGUI.indentLevel += 2;
            rootRotationBasedUpon = (RotationBasedUponOption)EditorGUILayout.EnumPopup("Based Upon", rootRotationBasedUpon, dropdownColor);
            EditorGUI.indentLevel -= 2;

            applyYPosition = EditorGUILayout.ToggleLeft("Bake Root Transform Position (Y)", applyYPosition);
            EditorGUI.indentLevel += 2;
            rootYBasedUpon = (PositionYBasedUponOption)EditorGUILayout.EnumPopup("Based Upon", rootYBasedUpon, dropdownColor);
            EditorGUI.indentLevel -= 2;

            applyXZPosition = EditorGUILayout.ToggleLeft("Bake Root Transform Position (XZ)", applyXZPosition);
            EditorGUI.indentLevel += 2;
            rootXZBasedUpon = (PositionXZBasedUponOption)EditorGUILayout.EnumPopup("Based Upon", rootXZBasedUpon, dropdownColor);
            EditorGUI.indentLevel -= 2;

            GUILayout.Space(5f);
            if (GUILayout.Button("Apply Naming Conventions", GUILayout.Height(25f))) ApplyNamingConventions();
            if (GUILayout.Button("Apply Animation Settings", GUILayout.Height(25f))) ApplyAnimationSettings();
            GUILayout.Space(5f);

            EditorGUILayout.LabelField($"Selected Importers ({importers.Length})", headerColor);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            //EditorGUILayout.BeginVertical(GUILayout.MaxHeight(69f));
            foreach (var obj in importers)
            {
                EditorGUILayout.ObjectField(obj, typeof(Object), false);
            }
            //EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void ApplyNamingConventions()
        {
            foreach (var importerObj in importers)
            {
                var modelImporter = GetModelImporter(importerObj);
                if (modelImporter == null) continue;

                Undo.RecordObject(modelImporter, "Apply Naming Conventions");

                var assetPath = AssetDatabase.GetAssetPath(importerObj);
                var fixedAssetName = ApplyNamingConvention(Path.GetFileNameWithoutExtension(assetPath));
                var newAssetPath = Path.Combine(Path.GetDirectoryName(assetPath), fixedAssetName + Path.GetExtension(assetPath));

                if (assetPath != newAssetPath)
                {
                    AssetDatabase.RenameAsset(assetPath, fixedAssetName);
                    Debug.Log($"Renamed asset to: {newAssetPath}");
                }

                var clips = GetClips(importerObj);
                foreach (var clip in clips)
                {
                    if (clip.name != fixedAssetName)
                    {
                        clip.name = fixedAssetName;
                        Debug.Log($"Renamed clip to: {fixedAssetName}");
                    }
                }

                modelImporter.clipAnimations = clips;
                modelImporter.SaveAndReimport();
            }
        }

        void ApplyAnimationSettings()
        {
            foreach (var importerObj in importers)
            {
                var modelImporter = GetModelImporter(importerObj);
                if (modelImporter == null) continue;

                Undo.RecordObject(modelImporter, "Apply Animation Settings");
                modelImporter.animationType = selectedAnimationType;

                var clips = GetClips(importerObj);
                foreach (var clip in clips)
                {
                    clip.loopTime = loopTime;
                    clip.mirror = mirror;
                    clip.lockRootRotation = applyRotation;
                    clip.lockRootHeightY = applyYPosition;
                    clip.lockRootPositionXZ = applyXZPosition;
                    ApplyBasedUponSettings(clip);
                }

                modelImporter.clipAnimations = clips;
                modelImporter.SaveAndReimport();
                Debug.Log($"Applied settings to {AssetDatabase.GetAssetPath(importerObj)}");
            }
        }

        void ApplyBasedUponSettings(ModelImporterClipAnimation clip)
        {
            clip.keepOriginalOrientation = rootRotationBasedUpon == RotationBasedUponOption.Original;
            clip.keepOriginalPositionY = rootYBasedUpon == PositionYBasedUponOption.Original;
            clip.heightFromFeet = rootYBasedUpon == PositionYBasedUponOption.Feet;
            clip.keepOriginalPositionXZ = rootXZBasedUpon == PositionXZBasedUponOption.Original;
        }

        string ApplyNamingConvention(string name)
        {
            // Keep original case, replace non-alphanumeric characters with '_', capitalize as needed
            return string.Concat(name.Select((ch, i) =>
                char.IsLetterOrDigit(ch) ? (i == 0 || name[i - 1] == '_' ? char.ToUpper(ch) : ch) : '_'));
        }

        ModelImporter GetModelImporter(Object importerObj) => AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(importerObj)) as ModelImporter;

        ModelImporterClipAnimation[] GetClips(Object importerObj) => GetModelImporter(importerObj)?.clipAnimations?.Length > 0 ? GetModelImporter(importerObj).clipAnimations : GetModelImporter(importerObj)?.defaultClipAnimations;
    }
}
