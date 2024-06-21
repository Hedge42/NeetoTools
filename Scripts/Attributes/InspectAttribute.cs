using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Matchwork
{
    /// <summary>
    /// Open properties window using <see cref="EditorUtility.OpenPropertyEditor(Object)"/>
    /// </summary>
    public class InspectAttribute : PropertyAttribute { }

    #region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectAttribute))]
    public class InspectAttributeDrawer : ObjectButtonDrawer
    {
        public override GUIContent content => new GUIContent(MTexture.inspect, "Inspect Object");
        public override void OnClick(SerializedProperty property) => EditorUtility.OpenPropertyEditor(property.objectReferenceValue);
    }
#endif
    #endregion

   
}