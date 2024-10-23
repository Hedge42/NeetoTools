using System;
using UnityEngine;
using Neeto;
using System.Linq;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
//using static NGUI;
using SceneAsset = UnityEditor.SceneAsset;
#endif

namespace Neeto
{
    [Serializable]
    public class SceneReference
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
    {
        public static implicit operator string(SceneReference _) => _.name;
        public string name;
        public string path;

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(SceneReference))]
        class SceneReferenceDrawer : PropertyDrawer
        {
            Texture no;
            Texture yes;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                no ??= EditorGUIUtility.IconContent("d_P4_DeletedLocal@2x").image;
                yes ??= EditorGUIUtility.IconContent("d_P4_CheckOutRemote@2x").image;

                var objProp = property.FindPropertyRelative(nameof(targetObject));
                var nameProp = property.FindPropertyRelative(nameof(name));
                var pathProp = property.FindPropertyRelative(nameof(path));
                var assetProp = property.FindPropertyRelative(nameof(asset));

                // update internal object reference
                if (objProp.objectReferenceValue != property.serializedObject.targetObject)
                {
                    objProp.objectReferenceValue = property.serializedObject.targetObject;
                    NGUI.ApplyAndMarkDirty(objProp);
                }

                // show scene name on hover
                label.tooltip = $"{nameProp.stringValue} ({pathProp.stringValue})";
                position.height = NGUI.lineHeight;

                EditorGUI.BeginProperty(position, label, property);

                // draw asset field
                position.height = NGUI.lineHeight;
                position.width -= 18f;
                EditorGUI.PropertyField(position, assetProp, label);

                // draw texture based on if scene is in build settings
                var isBuildScene = SceneHelper.IsSceneInBuildSettings(assetProp.objectReferenceValue as SceneAsset);
                var texture = isBuildScene ? yes : no;
                var colorMask = isBuildScene ? ColorWriteMask.Alpha | ColorWriteMask.Green : ColorWriteMask.Alpha | ColorWriteMask.Red;
                position.x += position.width + 2f;
                position.width = 16f;
                if (GUI.Button(position, GUIContent.none)) // open build settings button
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                EditorGUI.DrawTextureTransparent(position, texture, ScaleMode.ScaleToFit, 1f, 2f, colorMask);

                EditorGUI.EndProperty();
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return NGUI.fullLineHeight;
            }
        }

        static HashSet<SceneReference> instances = new();
        public SceneAsset asset;
        public Object targetObject;

        public void OnBeforeSerialize() { }

        // when the object is loaded or created
        public void OnAfterDeserialize()
        {
            /// use static instances to have 1-time subscription to  <see cref="EditorApplication.projectChanged"/>
            if (!instances.Contains(this))
            {
                instances.Add(this);
                EditorApplication.projectChanged += UpdateSceneName; // called when an asset is renamed
            }

            // ensure correct value on load
            // delayCall needed for access to asset.name within deserialization
            EditorApplication.delayCall += UpdateSceneName;
        }
        void UpdateSceneName()
        {
            var name = asset ? asset.name : "";
            var path = asset ? AssetDatabase.GetAssetPath(asset) : "";
            if (!name.Equals(this.name) || !path.Equals(this.path))
            {
                this.path = path;
                this.name = name;

                // may have not been applied yet
                if (targetObject) EditorUtility.SetDirty(targetObject);

                Debug.Log($"SceneReference updated: '{this.name}'");
            }
        }
#endif
    }
}