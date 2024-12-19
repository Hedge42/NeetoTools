using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    [CustomPropertyDrawer(typeof(DragBoxAttribute))]
    class DragBoxAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (NGUI.Property(position, property))
            {
                EditorGUI.PropertyField(position, property, label);
                var fieldType = fieldInfo.FieldType;

                if (NGUI.TryAcceptDragAndDrop(position, fieldInfo.FieldType, out var dropped))
                {
                    if (property.GetArrayProperty(out var arrayProperty))
                    {
                        var array = arrayProperty.GetProperValue(fieldInfo);
                        var e = array as IEnumerable<object>;

                        foreach (var obj in dropped)
                        {
                            e.Append(obj);
                        }

                        arrayProperty.SetProperValue(fieldInfo, System.Convert.ChangeType(e, array.GetType()));
                    }
                }
            }
        }


    }
}
