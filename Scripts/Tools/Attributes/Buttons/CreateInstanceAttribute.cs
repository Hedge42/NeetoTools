using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using static UnityEngine.GraphicsBuffer;
using Neeto;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

// https://www.youtube.com/watch?v=mCKeSNdO_S0&t=636s

public class ScriptableInstance : ScriptableObject
{
    [HideInInspector]
    public bool isUnique;
    [HideInInspector]
    public Object target;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
public class CreateInstanceAttribute : PropertyAttribute { }



#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CreateInstanceAttribute))]
public class CreateInstanceAttributeDrawer : PropertyButtonDrawerBase
{
    private Editor editor = null;
    private bool changed;

    public override GUIContent content => new GUIContent("+", "Create Internal Instance");

    public override void OnClick(SerializedProperty property)
    {
        //var debug = property.displayName;
        //var context = property.serializedObject.context;
        //var objectReference = property.objectReferenceValue as ScriptableObject;

        Undo.RecordObject(property.serializedObject.targetObject, property.displayName);

        if (property.objectReferenceValue)
        {
            property.objectReferenceValue = ScriptableObject.Instantiate(property.objectReferenceValue);
        }
        else
        {
            property.objectReferenceValue = ScriptableObject.Instantiate(ScriptableObject.CreateInstance(fieldInfo.FieldType));
        }

        if (property.objectReferenceValue)
            property.objectReferenceValue.name = $"({property.serializedObject.targetObject.name}) INSTANCE";

        //property
        NGUI.ApplyAndMarkDirty(property);

        //// clone existing if it exists
        //var instance = objectReference ? objectReference :  ScriptableObject.CreateInstance(fieldInfo.FieldType);
        //instance = ScriptableObject.Instantiate(instance);
        //// chatgpt'd "do I need to destroy these"? No.
        //// https://chat.openai.com/share/286c57dc-ee37-4d5f-a8af-4d4c9fa0667d

        ////EditorUtility.SetDirty(instance);

        //property.objectReferenceValue = instance;
        //EditorOps.ApplyAndMarkDirty(property);
    }

    public override bool IsEnabled(SerializedProperty property) => true;

    //public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //{

    //    // keep up to date if u use it
    //    var obj = property.objectReferenceValue;
    //    var fieldName = fieldInfo.FieldType.Name;
    //    var target = property.serializedObject.targetObject;

    //    position.width -= 38f;

    //    var buttonPosition = new Rect(
    //        // position
    //        new Vector2(position.xMax + 3, position.y),

    //        // size
    //        new Vector2(18, position.height));



    //    if (GUI.Button(buttonPosition, "+", Styles.iconButton))
    //    {
    //        Undo.RecordObject(property.serializedObject.targetObject, fieldName);

    //        // create new
    //        property.objectReferenceValue = ScriptableObject.Instantiate(ScriptableObject.CreateInstance(fieldInfo.FieldType));
    //        EditorOps.ApplyAndMarkDirty(property);
    //    }

    //    // export
    //    buttonPosition.x += 20f;
    //    EditorGUI.BeginDisabledGroup(!property.objectReferenceValue);
    //    if (GUI.Button(buttonPosition, Textures.save, Styles.iconButton))
    //    {
    //        var instance = MEditor.CreateAsset((ScriptableObject)property.objectReferenceValue);
    //        property.objectReferenceValue = instance;
    //        EditorOps.ApplyAndMarkDirty(property);
    //    }
    //    EditorGUI.EndDisabledGroup();


    //    EditorGUI.BeginChangeCheck();
    //    EditorGUI.PropertyField(position, property, label, true);
    //    if (EditorGUI.EndChangeCheck() || changed)
    //    {
    //        property.ApplyAndMarkDirty();
    //    }
    //}
}

[CustomPropertyDrawer(typeof(ScriptableInstance), true)]
public class ScriptableInstanceDrawer : PropertyDrawer
{
    private Editor editor = null;
    private bool changed;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // keep up to date if u use it

        EditorGUI.BeginChangeCheck();

        var pos = EndButtonsGUI(position, 1, out var buttonRects);
        EditorGUI.PropertyField(pos, property, label, true);

        var obj = property.objectReferenceValue;
        var fieldName = fieldInfo.FieldType.Name;
        var target = property.serializedObject.targetObject;
        var instance = obj as ScriptableInstance;

        if (instance)
        {
            instance.isUnique = ToggleButtonGUI(buttonRects[0], "u", instance.isUnique);

            if (instance.isUnique)
            {
                if (instance.target != property.serializedObject.targetObject)
                {
                    if (!CloneObject(property))
                        CreateObject(property);

                    // try to clone
                    if (changed = EditorGUI.EndChangeCheck()) // create clone
                    {
                        if (obj = property.objectReferenceValue)
                        {
                            Debug.Log($"Cloning ({fieldName}){obj.name}...", target);
                            obj = (Object)obj.JsonClone();
                            obj.name = "(instance)" + obj.name;

                            changed = true;
                            property.objectReferenceValue = obj;
                        }
                    }

                    // otherwise create instance
                    if (!obj && typeof(ScriptableInstance).IsAssignableFrom(fieldInfo.FieldType))
                    {
                        Debug.Log($"Creating {fieldName}", obj);
                        obj = ScriptableInstance.CreateInstance(fieldInfo.FieldType);
                        obj.name = $"(new) + {fieldName}";

                        changed = true;
                        property.objectReferenceValue = obj;
                    }
                }
            }
        }




        // foldout
        if (property.objectReferenceValue != null)
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none, true);
        else return;

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            if (!editor)
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

            //editor.ser
            EditorGUI.BeginChangeCheck();
            editor.OnInspectorGUI();
            changed = changed || EditorGUI.EndChangeCheck();
            EditorGUI.indentLevel--;
        }

        if (changed)
        {
            property.ApplyAndMarkDirty();
        }
    }

    void CreateInstance(SerializedProperty property)
    {
        if (!CloneObject(property))
            CreateObject(property);
    }

    private bool CreateObject(SerializedProperty property)
    {
        var obj = property.objectReferenceValue;
        if (!obj && typeof(ScriptableInstance).IsAssignableFrom(fieldInfo.FieldType))
        {
            Debug.Log($"Creating {property.name}", obj);
            obj = ScriptableObject.CreateInstance(fieldInfo.FieldType);
            obj.name = $"(new) + {property.name}";

            changed = true;
            property.objectReferenceValue = obj;
            property.FindPropertyRelative("target").objectReferenceValue = property.serializedObject.targetObject;

            return true;
        }

        return false;
    }

    private static bool CloneObject(SerializedProperty property)
    {
        var obj = property.objectReferenceValue;
        if (property.objectReferenceValue)
        {
            Debug.Log($"Cloning ({property.name}){obj.name}...", property.serializedObject.targetObject);
            obj = (Object)obj.JsonClone();
            obj.name = "(instance)" + obj.name;

            property.objectReferenceValue = obj;
            property.FindPropertyRelative("target").objectReferenceValue = property.serializedObject.targetObject;
            return true;
        }
        return false;
    }

    public static bool ToggleButtonGUI(Rect position, string text, bool value)
    {
        var temp = GUI.color;

        if (!value)
            GUI.color = new Color(.4f, .4f, .4f);

        if (GUI.Button(position, text))
        {
            value = !value;
        }

        GUI.color = temp;

        return value;
    }

    public static Rect EndButtonsGUI(Rect position, int count, out Rect[] rects)
    {
        var original = new Rect(position);
        var width = 20f;
        var totalWidth = count * width;

        rects = new Rect[count];

        position.xMin = position.xMax - totalWidth;
        position.xMax = position.xMin + width;

        for (int i = 0; i < count; i++)
        {
            rects[i] = position;
            position.x += position.width;
        }

        original.xMax -= totalWidth;
        return original;
    }
}




#endif

