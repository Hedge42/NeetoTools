using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomObjectPickerAttribute : PropertyAttribute
{
    public string filter { get; private set; }
    public CustomObjectPickerAttribute(string filter = null)
    {
        //this.pred = pred;
        this.filter = filter;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CustomObjectPickerAttribute))]
public class MyAttributeClassDrawer : PropertyDrawer
{
    CustomObjectPickerAttribute _atr;
    CustomObjectPickerAttribute atr => _atr == null ? _atr = attribute as CustomObjectPickerAttribute : _atr;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();

        // Create a custom button that will open the CustomObjectPicker window when clicked
        var btnPosition = new Rect(position);
        btnPosition.xMin = btnPosition.xMax - 20;
        if (EditorGUI.DropdownButton(btnPosition, GUIContent.none, FocusType.Passive))
        {
            CustomObjectPicker.ShowWindow(
                property,
                //typeof(MyCustomComponent),
                fieldInfo.FieldType,
            //obj => obj); // Replace "YourFilterString" with the string you want to match
                atr.filter == null ? null : obj => obj.name.Contains(atr.filter));
            //obj => obj.name.Contains(atr.filter)); // Replace "YourFilterString" with the string you want to match
        }

        EditorGUI.PropertyField(position, property, label);

        if (EditorGUI.EndChangeCheck())
        {
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
}


public class CustomObjectPicker : EditorWindow
{
    public static void ShowWindow(SerializedProperty property, System.Type type, System.Predicate<UnityEngine.Object> filter = null)
    {
        CustomObjectPicker window = CreateInstance<CustomObjectPicker>();
        window.Initialize(property, type, filter);
        window.ShowUtility();
    }

    private SerializedProperty property;
    private System.Type objectType;
    private System.Predicate<UnityEngine.Object> filter;
    private List<UnityEngine.Object> filteredObjects;
    private Vector2 scrollPosition;

    public void Initialize(SerializedProperty property, System.Type type, System.Predicate<UnityEngine.Object> filter = null)
    {
        this.property = property;
        this.objectType = type;
        this.filter = filter;

        titleContent = new GUIContent("Custom Object Picker");
        minSize = new Vector2(300, 300);
        maxSize = new Vector2(500, 500);

        //Debug.Log(objectType.IsAssignableFrom(typeof(FloatVariable)));
        Debug.Log(filter == null);

        string[] guids = AssetDatabase.FindAssets("t:UnityEngine.Object");
        filteredObjects = guids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path))
            .Where(obj => objectType.IsAssignableFrom(obj.GetType()))
            .Where(obj => filter == null || filter(obj))
            .ToList();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField($"Select a {objectType.Name}:", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var obj in filteredObjects)
        {
            if (GUILayout.Button(obj.name))
            {
                property.objectReferenceValue = obj;
                property.serializedObject.ApplyModifiedProperties();
                Close();
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
#endif