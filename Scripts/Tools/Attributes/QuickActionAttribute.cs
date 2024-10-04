using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class QuickActionAttribute : Attribute
    {
        public string file { get; private set; }
        public string label { get; private set; }
        public QuickActionAttribute(string file = null, string label = null)
        {
            if (file != null && !file.EndsWith(".cs"))
                file += ".cs";

            this.file = file;
        }

#if UNITY_EDITOR
        const string COPY_GUID = "Assets/Copy GUID";
        [MenuItem(COPY_GUID, validate = true, priority = 19)]
        static bool CanCopyGUID()
        {
            return Selection.activeObject;
        }
        [MenuItem(COPY_GUID, validate = false, priority = 19)]
        static void CopyGUID()
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log($"Copied Guid '{guid}' from '{Selection.activeObject.name}'", Selection.activeObject);
        }
#endif
    }
}
