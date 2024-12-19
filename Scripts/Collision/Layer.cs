/* https://answers.unity.com/questions/609385/type-for-layer-selection.html */

using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct Layer
{
    public static implicit operator int(Layer _) => _.layerIndex;
    public static implicit operator Layer(int _) => new(_);
    public Layer(int _layerIndex) => layerIndex = _layerIndex;
    public readonly int layerIndex;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Layer))]
public class SingleUnityLayerPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        SerializedProperty layerIndex = _property.FindPropertyRelative("layerIndex");
        if (layerIndex != null)
        {
            layerIndex.intValue = EditorGUI.LayerField(_position, _label, layerIndex.intValue);
        }
        EditorGUI.EndProperty();
    }
}
#endif

