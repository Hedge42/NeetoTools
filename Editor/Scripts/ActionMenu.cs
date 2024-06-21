using System;
using UnityEditor;
using UnityDropdown.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Linq.Expressions;

namespace Matchwork
{
    public class ActionMenu : DropdownMenu<Action>
    {
        public struct Item
        {
            public static implicit operator Item(Action action) => new Item { action = action.Invoke, path = action.Method.Name };
            public static implicit operator Item((Action action, string path) item) => new Item { action = item.action, path = item.path };
            public static implicit operator DropdownItem<Action>(Item item) => new DropdownItem<Action>(item.action, item.path);
            public Action action;
            public string path;
        }

        public ActionMenu(IList<DropdownItem<Action>> items, Action<Action> onValueSelected, int searchbarMinItemsCount = 10, bool sortItems = false, bool showNoneElement = false)
            : base(items, onValueSelected, searchbarMinItemsCount, sortItems, showNoneElement) { }


        public static ActionMenu Create(params Item[] items)
        {
            var menu = new ActionMenu
                (
                    items: items.Select(_ => (DropdownItem<Action>)_).ToList(),
                    onValueSelected: _ => _.Invoke(),
                    sortItems: true
                );

            menu.ShowAsContext();
            return menu;
        }
        //public static ActionMenu Create(params Action[] actions)
        //{
        //    var menu = new ActionMenu
        //        (
        //            items: actions.Select(_ => ((MenuAction)_).AsDropdownItem()).ToList(),
        //            onValueSelected: _ => _.Invoke(),
        //            sortItems: true
        //        );

        //    menu.ShowAsContext();
        //    return menu;
        //}
    }
}
