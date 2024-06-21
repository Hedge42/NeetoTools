//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using System.Threading;
//using Cysharp.Threading.Tasks;

//#if UNITY_EDITOR
//using UnityEditor;
//[CustomPropertyDrawer(typeof(SubComponentAttribute))]
//public class SubComponentDrawer : PropertyDrawer
//{
//    public static HashSet<SubComponentDrawer> all = new HashSet<SubComponentDrawer>();
//    public Component subComponent;
//    public MonoBehaviour mainComponent;
//    public Editor editor;
//    private Type subComponentType;

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        if (!all.Contains(this))
//        {
//            all.Add(this);
//            subComponentType = fieldInfo.FieldType;
//        }

//        mainComponent = (MonoBehaviour)property.serializedObject.targetObject;
//        subComponent = mainComponent.GetComponent(subComponentType);


//        if (subComponent == null)
//            subComponent = mainComponent.gameObject.AddComponent(subComponentType);

//        subComponent.hideFlags = HideFlags.HideInInspector;

//        if (subComponent != null)
//        {
//            Editor.CreateCachedEditor(subComponent, null, ref editor);

//            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
//            if (property.isExpanded)
//            {
//                EditorGUI.indentLevel++;
//                editor.OnInspectorGUI();
//                EditorGUI.indentLevel--;
//            }
//        }
//    }
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return 0f;
//    }
//}
//#endif

///// <summary>
///// This still has some issues:
///// <br/> what if the component already exists? Support components allowing multiple on the same gameObject?
///// <br/> always create new?
///// <br/> what if there are requireComponent dependencies?
///// </summary>
//public class SubComponentAttribute : PropertyAttribute
//{

//    // InitializeOnLoadMethod would not be called from propertydrawer class
//#if UNITY_EDITOR
//    [InitializeOnLoadMethod]
//    public static void CheckHostComponents()
//    {
//        EditorApplication.update += () =>
//        {
//            try
//            {

//                var remove = new List<SubComponentDrawer>();
//                foreach (var drawer in SubComponentDrawer.all)
//                {
//                    // The MainComponent has been destroyed
//                    if (drawer != null && drawer.subComponent != null && drawer.mainComponent == null)
//                    {
//                        // delayCall removes editor errors
//                        EditorApplication.delayCall += () =>
//                        {
//                            Editor.DestroyImmediate(drawer.subComponent);
//                            Editor.DestroyImmediate(drawer.editor);
//                        };
//                        remove.Add(drawer);
//                    }
//                }

//                foreach (var t in remove)
//                    SubComponentDrawer.all.Remove(t);
//            }
//            catch { }
//        };
//    }
//#endif
//}
