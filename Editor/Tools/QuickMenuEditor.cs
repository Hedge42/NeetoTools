using UnityEditor;
using UnityDropdown;
using UnityToolbarExtender;
using System.Linq;
using System.Reflection;
using UnityDropdown.Editor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neeto
{
    static class QuickMenuEditor
    {
        struct Entry
        {
            public Action action;
            public Func<string> getLabel;
            public int order;
            QuickMenuAttribute attribute;

            public DropdownItem<Action> GetItem() => new DropdownItem<Action>(action, getLabel());
            public Entry(MethodInfo method)
            {
                attribute = method.GetCustomAttribute<QuickMenuAttribute>();
                order = attribute.order;
                getLabel = attribute.getLabel ?? new Func<string>(() => method.Name);
                action = () => method.Invoke(null, null);
            }
        }

        static IList<Entry> entries;

        [InitializeOnLoadMethod]
        static void Awake()
        {
            entries = TypeCache.GetMethodsWithAttribute(typeof(QuickMenuAttribute)).Select(method => new Entry(method)).OrderBy(e => e.order).ToList();

            ToolbarExtender.LeftToolbarGUI.Add(() =>
            {
                GUILayout.Space(4f);
                if (NGUI.ToolbarButton(EditorGUIUtility.IconContent("Favorite On Icon")))
                {
                    var items = entries.Select(e => e.GetItem()).ToList();
                    new DropdownMenu<Action>(items, _ => _.Invoke()).ShowAsContext();
                }
            });
        }
    }
}