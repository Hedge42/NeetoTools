using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Matchwork
{
    public class ConditionalAttribute : PropertyAttribute
    {
        const BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.GetProperty |
            BindingFlags.GetField;

        public Func<Object, bool> GetConditional { get; private set; }

        public ConditionalAttribute(string propertyName)
        {

            GetConditional = obj =>
            {
                var type = obj.GetType();
                var fieldInfo = type.GetField(propertyName, flags);
                if (fieldInfo != null && fieldInfo.FieldType.Equals(typeof(bool)))
                    return (bool)fieldInfo.GetValue(obj);

                var propertyInfo = type.GetProperty(propertyName, flags);
                if (propertyInfo != null && propertyInfo.PropertyType.Equals(typeof(bool)))
                    return (bool)propertyInfo.GetValue(obj);


                Debug.LogError($"No boolean field or property found matching {type.Name}.{propertyName}");
                return true;
            };
        }
    }
}