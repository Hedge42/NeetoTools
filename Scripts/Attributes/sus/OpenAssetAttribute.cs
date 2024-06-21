using Matchwork;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Displays a button next to an Object reference calling:<br/>
/// <see cref="AssetDatabase.OpenAsset(Object)"/>
/// </summary>
public class OpenAssetAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OpenAssetAttribute))]
public class OpenAssetDrawer : ObjectButtonDrawer
{
    public override GUIContent content => new GUIContent(MTexture.click, "Open Asset");
    public override void OnClick(SerializedProperty property) => AssetDatabase.OpenAsset(property.objectReferenceValue);
}
#endif

