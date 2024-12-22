using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Neeto
{
    using static System.Reflection.BindingFlags;
    public class FieldSelectorAttribute
    {
        public readonly Type reflectedType;
        public readonly Type fieldType;
        public readonly BindingFlags flags;
        public FieldSelectorAttribute(Type reflectedType, Type fieldType, BindingFlags flags = Public | Static | FlattenHierarchy)
        {
            this.reflectedType = reflectedType;
            this.fieldType = fieldType;
            this.flags = flags;
        }
    }
}
