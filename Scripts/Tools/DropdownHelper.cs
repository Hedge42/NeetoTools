using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Neeto
{
    /// <summary>
    /// Create custom DropdownMenu from runtime assembly
    /// </summary>
    public static class DropdownHelper
    {
        static readonly Type ItemType = Type.GetType("UnityDropdown.Editor.DropdownItem`1, UnityDropdown.Editor");
        static readonly Type MenuType = Type.GetType("UnityDropdown.Editor.DropdownMenu`1, UnityDropdown.Editor");
        static Type GetItemType<T>() => ItemType.MakeGenericType(typeof(T));
        static Type GetMenuType<T>() => MenuType.MakeGenericType(typeof(T));

        public static void Show<T>(Action<T> callback, bool sortItems, bool showNoneElement, string selected, params (T context, string text)[] items)
        {
            var itemType = GetItemType<T>();
            var menuType = GetMenuType<T>();

            // Create the list of DropdownItems
            var itemList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

            foreach (var (context, text) in items)
            {
                var dropdownItem = Activator.CreateInstance(itemType, context, text, null, null, selected.Equals(text));
                itemList.Add(dropdownItem);
            }

            // Create DropdownMenu with the items list and a callback action
            var dropdownMenu = Activator.CreateInstance(menuType, itemList, callback, 10, sortItems, showNoneElement);
            var showAsContextMethod = menuType.GetMethod("ShowAsContext", BindingFlags.Instance | BindingFlags.Public);
            showAsContextMethod.Invoke(dropdownMenu, new object[] { 0 });
        }
    }
}
