using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{

    /// <summary>
    /// Open properties window using <see cref="EditorUtility.OpenPropertyEditor(Object)"/>
    /// </summary>
    public class InspectAttribute : PropertyAttribute { }

    #region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectAttribute))]
    public class InspectAttributeDrawer : PropertyButtonDrawerBase
    {
        public override GUIContent content => EditorGUIUtility.IconContent("d_SearchQueryAsset Icon").With(tooltip: "Inspect");
        public override void OnClick(SerializedProperty property) => EditorUtility.OpenPropertyEditor(property.objectReferenceValue);
    }
#endif
    #endregion
}