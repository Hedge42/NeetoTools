using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Neeto
{
#if UNITY_EDITOR
    public static class ManagedReferenceCopyPaste
    {
        public static object buffer;
        public static void ContextMenu(SerializedProperty property, Rect rect)
        {
            Event evt = Event.current;

            if (evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
            {
                GenericMenu menu = new GenericMenu();

                new ManagedReferenceContextMenu().GetItems(menu, property);

                menu.ShowAsContext();

                evt.Use(); // Mark the event as used
            }
        }

        public struct ManagedReferenceContextMenu : IContextMenu
        {
            public bool GetItems(GenericMenu menu, SerializedProperty property)
            {
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    menu.AddItem(new GUIContent("Copy managedReferenceValue"), false, () => CopyManagedReferenceValue(property));
                    if (buffer != null && PolymorphicDrawer.GetManagedReferenceFieldType(property).IsAssignableFrom(buffer.GetType()))
                        menu.AddItem(new GUIContent("Paste managedReferenceValue"), false, () => PasteManagedReferenceValue(property));
                    return true;
                }
                return false;
            }
        }
        public interface IContextMenu
        {
            bool GetItems(GenericMenu menu, SerializedProperty property);
        }
        public static void CopyManagedReferenceValue(SerializedProperty property)
        {
            buffer = (object)property.managedReferenceValue;
        }
        public static void PasteManagedReferenceValue(SerializedProperty property)
        {
            if (buffer != null)
            {
                // undo support
                property.serializedObject.Update();
                Undo.RecordObject(property.serializedObject.targetObject, "Paste property");

                // creates copy
                var json = JsonUtility.ToJson(buffer);
                var value = JsonUtility.FromJson(json, buffer.GetType());

                // set value
                property.managedReferenceValue = value; // Adjust this for different property types
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}