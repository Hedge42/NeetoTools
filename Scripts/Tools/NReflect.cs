//using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;
using Neeto;
using System.IO;
using UnityEngine;


namespace Neeto
{
    public static class NReflect
    {
        public static bool TryGetAttribute<T>(this MemberInfo mem, out T attribute, bool inherit = true) where T : Attribute
        {
            return null != (attribute = mem.GetCustomAttribute<T>(inherit));
        }
        public static IEnumerable<MethodInfo> GetMethods(this Module pkg, BindingFlags flags = BindingFlags.Default)
        {
            return pkg.GetTypes().SelectMany(t => t.GetMethods(flags));
        }
        public static IEnumerable<PropertyInfo> GetProperties(this Module pkg, BindingFlags flags = BindingFlags.Default)
        {
            return pkg.GetTypes().SelectMany(t => t.GetProperties(flags));
        }
        public static IEnumerable<EventInfo> GetEvents(this Module pkg, BindingFlags flags = BindingFlags.Default)
        {
            return pkg.GetTypes().SelectMany(type => type.GetEvents(flags));
        }

        public static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            if (type == null && !string.IsNullOrWhiteSpace(typeName))
                Debug.LogWarning($"{typeName} not found");

            return null;
        }

        public static List<FieldInfoWithTypeArguments> GetFieldsInheritingFromBaseType<T>(this Type classType)
        {
            var result = new List<FieldInfoWithTypeArguments>();
            foreach (FieldInfo field in classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition().IsSubclassOf(typeof(T)))
                {
                    var fieldWithTypeArguments = new FieldInfoWithTypeArguments
                    {
                        FieldInfo = field,
                        GenericTypeArguments = field.FieldType.GetGenericArguments()
                    };

                    result.Add(fieldWithTypeArguments);
                }
                else if (typeof(T).IsAssignableFrom(field.FieldType))
                {
                    var fieldWithTypeArguments = new FieldInfoWithTypeArguments
                    {
                        FieldInfo = field,
                        GenericTypeArguments = Type.EmptyTypes
                    };

                    result.Add(fieldWithTypeArguments);
                }
            }

            return result;
        }

        public static string ToSignature(this MethodInfo info)
        {
            StringBuilder signature = new StringBuilder();

            // Append type name
            signature.Append(info.DeclaringType.FullName).Append('.');

            // Append method name
            signature.Append(info.Name);

            // Append parameter types
            var paramTypes = info.GetParameters().Select(p => p.ParameterType.FullName);
            signature.Append('(').Append(string.Join(",", paramTypes)).Append(')');

            // Debug.Log(methodInfo.Name);

            return signature.ToString();
        }
        public static string ToSignature(ConstructorInfo ConstructorInfo)
        {

            StringBuilder signature = new StringBuilder();

            // Append type name
            signature.Append(ConstructorInfo.DeclaringType.FullName);

            // Append parameter types
            var paramTypes = ConstructorInfo.GetParameters().Select(p => p.ParameterType.FullName);
            signature.Append('(').Append(string.Join(",", paramTypes)).Append(')');

            return signature.ToString();
        }
        public static string ToSignature(this PropertyInfo info)
        {
            StringBuilder signature = new StringBuilder();

            // Append type name
            signature.Append(info.DeclaringType.FullName).Append('/');

            // Append method name
            signature.Append(info.Name);

            // Append parameter types
            signature.Append(" => ");
            signature.Append(info.PropertyType.FullName);

            // Debug.Log(methodInfo.Name);

            return signature.ToString();
        }
        public static string ToSignature(this EventInfo info)
        {
            StringBuilder signature = new StringBuilder();

            // Append type name
            signature.Append(info.DeclaringType.FullName).Append('/');

            // Append method name
            signature.Append(info.Name);

            Debug.Log(info.EventHandlerType);

            // Append parameter types
            //signature.Append(" => ");
            //signature.Append(info.PropertyType.FullName);

            // Debug.Log(methodInfo.Name);

            return signature.ToString();
        }

        public static MethodInfo ToMethod(string signature)
        {
            try
            {

                var typeAndMethod = signature.Split('(');
                var typeAndMethodParts = typeAndMethod[0].Split('.');
                var methodName = typeAndMethodParts.Last();

                var typeParts = typeAndMethodParts.Take(typeAndMethodParts.Length - 1);
                var typeName = string.Join('.', typeParts);
                var type = NReflect.GetType(typeName);
                var paramsFull = typeAndMethod[1].TrimEnd(')');

                // Split would create an empty string where it wasn't needed...
                if (!string.IsNullOrWhiteSpace(paramsFull))
                {
                    var paramTypes = paramsFull.Split(',').Select(n => GetType(n)).ToArray();
                    return type.GetMethod(methodName, paramTypes);
                }
                else
                {
                    return type.GetMethod(methodName, new Type[] { });
                }
            }
            catch
            {
                return null;
            }
        }
        public static bool ToMethod(string signature, out MethodInfo info)
        {
            info = default;
            try
            {
                info = ToMethod(signature);
                return info != null;
            }
            catch
            {
                return false;
            }
        }
        public static bool ToEvent(string signature, out EventInfo info)
        {
            try
            {
                var targetType = signature.Split('/')[0];
                var eventName = signature.Split('/')[1];

                info = NReflect.GetType(targetType).GetEvent(eventName);

                return info != null;
            }
            catch
            {
                info = null;
                return false;
            }
        }
        public static ConstructorInfo ToConstructor(string signature)
        {
            try
            {
                var parts = signature.Split('(');
                var type = Type.GetType(parts[0]);

                if (type == null) throw new ArgumentException($"Type {type.Name} not found.");
                var paramParts = parts[1].TrimEnd(')');

                // Split would create an empty string where it wasn't needed...
                if (!string.IsNullOrWhiteSpace(paramParts))
                {
                    var paramTypes = paramParts.Split(',').Select(n => GetType(n)).ToArray();
                    return type.GetConstructor(paramTypes);
                }
                else
                {
                    return type.GetConstructor(new Type[] { });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not get constructor from '{signature}'\n{e.Message}\n{e.StackTrace}");
                return null;
            }
        }
        public static PropertyInfo ToProperty(string signature)
        {
            ToProperty(signature, out var result);
            return result;
        }
        public static bool ToProperty(string signature, out PropertyInfo info)
        {
            try
            {
                var typeAndName = signature.Split(" => ")[0].Split('/');
                var typeName = typeAndName[0];
                var propName = typeAndName[1];
                var type = Module.ALL.GetType(typeName);
                info = type.GetProperty(propName, GameCallback.FLAGS_P);
                return info != null;
            }
            catch
            {
                Debug.LogError($"Error getting property from signature '{signature}'");
                info = null;
                return false;
            }
        }



        public static Type[] GetParameterTypes(this MethodInfo m)
        {
            return m.GetParameters().Select(p => p.ParameterType).ToArray();
        }
        public static Type[] GetParameterTypesWithTarget(this MethodInfo m)
        {
            var types = m.GetParameters().Select(p => p.ParameterType);
            if (!m.IsStatic)
            {
                types.Prepend(m.DeclaringType);
            }
            return types.ToArray();
        }
        public static bool ContainsTargetableParameter(this MethodInfo info, Type[] types)
        {
            var list = new List<Type>(types);

            foreach (var t in GetParameterTypesWithTarget(info))
            {
                if (list.Contains(t))
                {
                    list.Remove(t);
                }
            }

            return list.Count == 0;
        }
        public static IEnumerable<(string name, Type type)> GetParameterTypesWithNames(this MethodInfo m)
        {
            return m.GetParameters().Select(p => (p.Name, p.ParameterType));
        }
        public static string BackingField(string variableName)
        {
            return string.Format("<{0}>k__BackingField", variableName);
        }
        public static FieldInfo GetField(string propertyName, object obj)
        {
            return obj.GetType().GetField(BackingField(propertyName), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static T[] GetConstants<T>(Type declaringType)
        {
            return declaringType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                       .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                       .Select(fi => (T)fi.GetRawConstantValue())
                       .ToArray();
        }
        public static T UnpackConstant<T>(Type declaringType, string name)
        {
            return (T)declaringType.GetField(name).GetRawConstantValue();
        }
        public static bool HasAttribute<T>(object obj) where T : Attribute
        {
            return obj != null && obj.GetType().GetCustomAttribute<T>() != null;
        }
        public static bool HasAttribute<T>(this MemberInfo info) where T : Attribute
        {
            return info.GetCustomAttribute<T>() != null;
        }
        public static bool HasAttribute<T>(this MemberInfo info, out T result) where T : Attribute
        {
            return null != (result = info.GetCustomAttribute<T>());
        }
        public static string FileName(this string str)
        {
            return Path.GetFileNameWithoutExtension(str);
        }

        public static GUIContent DropdownContent(this MemberInfo info)
        {

            if (info == null)
                return new GUIContent(":(");

            var content = new GUIContent($"{info.DeclaringType.Name}.{info.Name}");

            if (info is MethodInfo method)
            {
                content.text = $"{content.text} {string.Join(',', method.GetParameterTypes().Select(t => t.Name))}";
            }
            return content;
        }
        public static string ModuleName(this MemberInfo info)
        {
            return Path.GetFileNameWithoutExtension(info.Module.Name);
        }
        public static string DropdownPath(this MemberInfo info)
        {
            return info.Module.Name.FileName() + "/";
        }
        public static bool HasAttribute<T>(object obj, out T result) where T : Attribute
        {
            result = default;
            if (obj == null)
                return false;

            result = obj.GetType().GetCustomAttribute<T>();
            return result != null;
        }

        public static IEnumerable<Type> GetAssignableReferenceTypes(Type fieldType, List<Func<Type, bool>> filters = null)
        {
            var appropriateTypes = new List<Type>();

            try
            {
                if (fieldType != null)
                {

                    // Get and filter all appropriate types
                    foreach (var type in TypeCache.GetTypesDerivedFrom(fieldType))
                    {
                        if (!IsActivatable(type))
                            continue;

                        // Filter types by provided filters if there is ones
                        if (filters != null && filters.All(f => f == null || f.Invoke(type)) == false)
                            continue;

                        appropriateTypes.Add(type);
                    }
                }
            }
            catch
            {
                Debug.Log(fieldType);
            }
            finally
            {
                if (IsActivatable(fieldType))
                    appropriateTypes.Insert(0, fieldType);
            }


            return appropriateTypes;
        }

        public static bool IsActivatable(Type type)
        {
            if (type == null)
                return false;
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                return false;
            if (type.IsAbstract)
                return false;
            if (type.ContainsGenericParameters)
                return false;
            if (type.IsClass && type.GetConstructor(Type.EmptyTypes) == null) // Structs still can be created (strangely)
                return false;
            if (!type.IsVisible)
                return false;
            if (type.IsInterface)
                return true;
            if (!type.IsSerializable)
                return false;

            return true;
        }

        public static string GetInheritingString(this Type type)
        {
            var baseString = "";
            var interfaces = type.GetInterfaces();
            if (type.BaseType != null && type.BaseType != typeof(System.Object))
            {
                baseString += " : " + type.BaseType.Name;
            }
            if (interfaces.Length > 0)
            {
                baseString += baseString.IsEmpty() ? " : " : ", ";

                for (int i = 0; i < interfaces.Length; i++)
                {
                    var interfaceType = interfaces[i];
                    baseString += interfaceType.Name;
                    if (i + 1 < interfaces.Length)
                        baseString += ", ";
                }
            }
            return baseString;
        }
        public static string GetDeclaringString(this Type type)
        {
            var output = type.Name;
            while (type.DeclaringType != null)
            {
                output = type.DeclaringType.Name + "." + output;
                type = type.DeclaringType;
            }
            return output;
        }


#if UNITY_EDITOR
        public static object GetValue(object source, string name)
        {
            if (source == null)
                return null;

            var type = source.GetType();
            var field = type.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var prop = type.GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

            if (field != null)
                return field.GetValue(source);

            return prop?.GetValue(source, null);
        }
        public static object GetValue(object source, string name, int index)
        {
            var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
            var enm = enumerable?.GetEnumerator();

            while (index-- >= 0)
            {
                enm?.MoveNext();
            }

            return enm?.Current;
        }
        public static MethodInfo FindMethod(this SerializedProperty property, string methodName, out object targetObject)
        {
            targetObject = property.serializedObject.targetObject;
            MethodInfo method = targetObject?.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method != null)
                return method;

            // If not found, traverse parent properties' managedReferenceValues
            SerializedProperty parentProperty = property.Copy();
            while (parentProperty.depth > 0)
            {
                parentProperty = Parent(parentProperty);
                targetObject = parentProperty?.managedReferenceValue;
                if (targetObject != null)
                {
                    method = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method != null)
                        return method;
                }
            }

            targetObject = null;
            return null;
        }
        public static object FindMethodTarget(this SerializedProperty property, string methodName, out MethodInfo method)
        {
            object targetObject = property.serializedObject.targetObject;
            method = targetObject?.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (method != null)
                return method;

            // If not found, traverse parent properties' managedReferenceValues
            SerializedProperty parentProperty = property.Copy();
            while (parentProperty.depth > 0)
            {
                parentProperty = Parent(parentProperty);
                targetObject = parentProperty?.managedReferenceValue;
                if (targetObject != null)
                {

                    method = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (method != null)
                        return targetObject;
                }
            }

            return null;
        }
        public static object FindFieldTarget(this SerializedProperty property, FieldInfo info)
        {
            object target = property.serializedObject.targetObject;


            FieldInfo field = target.GetType().GetField(info.Name);

            if (field != null && field.Name.Equals(info.Name))
            {
                return info.GetValue(target);
            }

            // If not found, traverse parent properties' managedReferenceValues
            //property = property.Copy();
            while (property.depth > 0)
            {
                property = property.Parent();
                if (property?.managedReferenceValue != null)
                {
                    return property.managedReferenceValue;
                }
            }

            return null;
        }
        public static object FindReflectionTarget(this SerializedProperty property, FieldInfo info)
        {
            /*
             * FieldInfo target is not compatible with Unity's target*/
            object target = property.serializedObject.targetObject;

            var field = target.GetType().GetField(info.Name);
            if (field != null && field.Name.Equals(info.Name))
            {
                return info.GetValue(target);
            }
            /* expected fieldInfo target is not the targetObject...
            */

            // If not found, traverse up 
            //property = property.Copy();
            while (property.depth > 0)
            {
                property = property.Parent();

                if (property == null)
                    continue;
                if (property.propertyType != SerializedPropertyType.ManagedReference)
                    continue;
                if (property.managedReferenceValue == null)
                    continue;

                if (info.GetValue(property.managedReferenceValue) != null)
                    return property.managedReferenceValue;
            }

            return null;
        }
        public static object GetValue(this FieldInfo info, SerializedProperty property)
        {
            return info.GetValue((object)FindReflectionTarget(property, info));
        }
        public static SerializedProperty Parent(this SerializedProperty property)
        {
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot == -1) return null;
            string parentPath = path.Substring(0, lastDot);
            return property.serializedObject.FindProperty(parentPath);
        }
#endif
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class ReflectionCache
    {
        static Dictionary<string, MethodInfo> methods;
        static ReflectionCache() => methods = new Dictionary<string, MethodInfo>();

        public static MethodInfo GetMethod(string signature)
        {
            MethodInfo method;
            if (!methods.TryGetValue(signature, out method))
                NReflect.ToMethod(signature, out method);

            return method;
        }
    }

    public class FieldInfoWithTypeArguments
    {
        public FieldInfo FieldInfo { get; set; }
        public Type[] GenericTypeArguments { get; set; }
    }
}