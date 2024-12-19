using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Reflection;
using static Codice.CM.Common.CmCallContext;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Matchwork
{
    #region EDITOR
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DragBoxAttribute))]
    public class DragBoxAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //if (property.isArray)
            //Debug.Log("ayyyiouaef");

            EditorGUI.PropertyField(position, property, label);
        }

        public static bool AcceptDrag<T>(Rect dropArea, out T[] dads)
        {
            dads = DragAndDrop.objectReferences.Where(obj => obj is T).Select(obj => (T)(object)obj).ToArray();

            if (dads?.Length == 0)
                return false;

            var e = Event.current;
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!dropArea.Contains(e.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (e.type != EventType.DragPerform)
                        break;

                    DragAndDrop.AcceptDrag();
                    e.Use();
                    return true;
                default:
                    break;
            }

            return false;
        }
    }
#endif
    #endregion

    [Serializable]
    public class DragBoxAttribute : PropertyAttribute
    {
        public DragBoxAttribute()
        {
        }
    }
}