using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using Rhinox.Lightspeed.Editor;
using Rhinox.Lightspeed.Reflection;

#if UNITY_EDITOR
//using Neeto.Toolbox;
using UnityEditor;
#endif


namespace Neeto
{
    public static class NGUI
    {
        #region RUNTIME
        static Texture2D _shadow, _highlight;
        public static Texture2D shadow => _shadow ??= Color.black.With(a: .1f).AsTexturePixel();
        public static Texture2D highlight => _highlight ??= Color.white.With(a: .06f).AsTexturePixel();

        public static readonly Color Shadow = Color.black.With(a: .1f);
        public static readonly Color Light = Color.white.With(a: .06f);

        public static Rect LerpRect(Rect rectA, Rect rectB, float t)
        {
            var result = new Rect();
            result.position = Vector2.Lerp(rectA.position, rectB.position, t);
            result.size = Vector2.Lerp(rectA.size, rectB.size, t);
            return result;
        }
        public static Vector2 SetTextAndUpdateWidth(this TextMeshProUGUI tmp, string text, float margins = 0f)
        {
            tmp.text = text;
            var size = tmp.rectTransform.sizeDelta;
            size.x = Mathf.Max(tmp.preferredWidth + margins * 2, size.y);
            return tmp.rectTransform.sizeDelta = size;
        }
        public static void SetTextAndUpdateHeight(this TextMeshProUGUI tmp, string text)
        {
            tmp.text = text;
            var size = tmp.rectTransform.sizeDelta;
            size.y = tmp.preferredHeight;
            tmp.rectTransform.sizeDelta = size;
        }
        #endregion RUNTIME

        #region RUNTIME_REFLECTION
        public static void CopyTo<T>(this T source, T dest)
        {
            var type = typeof(T);

            // Copy all fields
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                field.SetValue(dest, field.GetValue(source));
            }

            // Copy all properties
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.CanWrite)
                {
                    property.SetValue(dest, property.GetValue(source));
                }
            }
        }
        public static bool TryGetValue<T>(this PropertyInfo info, object instance, out T result)
        {
            result = default;
            if (info == null)
                return false;

            result = (T)info.GetValue(instance);
            return result is T;
        }
        public static bool TryGetValue<T>(this FieldInfo info, object instance, out T result)
        {
            result = default;
            if (info == null)
                return false;

            result = (T)info.GetValue(instance);
            return result is T;
        }
        public static bool TryGetValue<T>(this MethodInfo info, object instance, out T result, params object[] args)
        {
            result = default;
            if (info == null)
                return false;

            if (!typeof(T).IsAssignableFrom(info.ReturnType))
                return false;

            result = (T)info.Invoke(instance, args);
            return result is T;
        }
        public static bool TryGetValue<T>(this MemberInfo info, object instance, out T result)
        {
            result = default;

            return info switch
            {
                MethodInfo m => TryGetValue(m, instance, out result, new object[0]),
                PropertyInfo p => TryGetValue(p, instance, out result),
                FieldInfo f => TryGetValue(f, instance, out result),
                _ => false
            };
        }
        public static bool TryGetAttribute<T>(this MemberInfo mem, out T attribute, bool inherit = true) where T : Attribute
        {
            return null != (attribute = mem.GetCustomAttribute<T>(inherit));
        }

        public static BindingFlags PrivateStatic => BindingFlags.NonPublic | BindingFlags.Static;
        public static BindingFlags Static => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        public static IEnumerable<Assembly> RuntimeAssemblies => Factory.Cache(nameof(RuntimeAssemblies), GetRuntimeAssemblies);
        public static IEnumerable<Assembly> GetRuntimeAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(ass => (ass, ass.GetName().Name.ToLower()))
                .Where(ass_name => !ass_name.Item2.Contains("editor") && !ass_name.Item2.Contains("test"))
                .Select(ass_name => ass_name.Item1);
        }
        public static IEnumerable<MethodInfo> GetMethods(BindingFlags flags = BindingFlags.Default)
        {
            return RuntimeAssemblies.SelectMany(asm => asm.GetTypes()).GetMethods(flags);
        }
        public static IEnumerable<MethodInfo> GetMethods(this IEnumerable<Assembly> assemblies, BindingFlags flags = BindingFlags.Default)
        {
            return assemblies.SelectMany(asm => asm.GetTypes()).GetMethods(flags);
        }
        public static IEnumerable<MethodInfo> GetMethods(this IEnumerable<Type> types, BindingFlags flags = BindingFlags.Default)
        {
            return types.SelectMany(t => t.GetMethods(flags));
        }
        public static IEnumerable<PropertyInfo> GetProperties(Type returnType, BindingFlags flags = BindingFlags.Default)
        {
            return GetProperties(RuntimeAssemblies, returnType, flags);
        }
        public static IEnumerable<PropertyInfo> GetProperties(this IEnumerable<Assembly> assemblies, Type returnType, BindingFlags flags = BindingFlags.Default)
        {
            return assemblies.SelectMany(asm => asm.GetTypes())
                .GetProperties(returnType, flags);
        }
        public static IEnumerable<PropertyInfo> GetProperties(this IEnumerable<Type> types, Type returnType, BindingFlags flags = BindingFlags.Default)
        {
            return types.SelectMany(t => t.GetProperties(flags))
                .Where(property => returnType.IsAssignableFrom(property.PropertyType));
        }
        public static IEnumerable<EventInfo> GetEvents(BindingFlags flags = BindingFlags.Default)
        {
            return RuntimeAssemblies.SelectMany(asm => asm.GetTypes()).GetEvents(flags);
        }
        public static IEnumerable<EventInfo> GetEvents(this IEnumerable<Assembly> assemblies, BindingFlags flags = BindingFlags.Default)
        {
            return assemblies.SelectMany(asm => asm.GetTypes()).GetEvents(flags);
        }
        public static IEnumerable<EventInfo> GetEvents(this IEnumerable<Type> types, BindingFlags flags = BindingFlags.Default)
        {
            return types.SelectMany(t => t.GetEvents(flags));
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
        public static List<(FieldInfo, Type[])> GetFieldsInheritingFromBaseType<T>(this Type classType)
        {
            var result = new List<(FieldInfo, Type[])>();
            foreach (FieldInfo field in classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition().IsSubclassOf(typeof(T)))
                {
                    result.Add((field, field.FieldType.GetGenericArguments()));
                }
                else if (typeof(T).IsAssignableFrom(field.FieldType))
                {
                    result.Add((field, Type.EmptyTypes));
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
                var type = NGUI.GetType(typeName);
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

                info = NGUI.GetType(targetType).GetEvent(eventName);

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
                var type = NGUI.GetType(typeName);
                info = type.GetProperty(propName, GameCallback.FLAGS_P);
                return info != null;
            }
            catch
            {
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
        public static IEnumerable<T> GetConstants<T>(Type declaringType)
        {
            return declaringType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                       .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                       .Select(fi => (T)fi.GetRawConstantValue());
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
        public static bool HasAttribute<T>(object obj, out T result) where T : Attribute
        {
            result = default;
            if (obj == null)
                return false;

            result = obj.GetType().GetCustomAttribute<T>();
            return result != null;
        }
        public static bool IsActivatable(this Type type)
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
        public static IEnumerable<Type> GetRuntimeTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.GetName().Name.ToLower().Contains("editor"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.FullName.ToLower().Contains("editor"));
        }
        #endregion

        #region STRINGS
        public static bool EqualsAny(this string s, params string[] arr) => arr.Any(_ => _.Equals(s));
        public static string WithHTML(this string str, Color? color = null, bool bold = false, bool italic = false, uint? size = null)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (color.HasValue)
            {
                Color c = color.Value;
                string hexColor = ColorUtility.ToHtmlStringRGBA(c);
                str = $"<color=#{hexColor}>{str}</color>";
            }

            if (size != null)
                str = $"<size={size}>{str}</size>";

            if (bold)
                str = $"<b>{str}</b>";

            if (italic)
                str = $"<i>{str}</i>";

            return str;
        }
        public static string WithPrefix(this string _, string prefix)
        {
            if (_.StartsWith(prefix))
                return _;
            else
                return prefix + _;
        }
        public static string WithSuffix(this string _, string suffix)
        {
            _ = _.WithoutExtension(out var ext);

            if (_.EndsWith(suffix))
                return _ + ext;
            else
                return _ + suffix + ext; ;
        }
        public static string WithoutExtension(this string path)
        {
            int dotIndex = path.LastIndexOf('.');
            return dotIndex >= 0 ? path.Substring(0, dotIndex) : path;
        }
        public static string WithoutExtension(this string path, out string extension)
        {
            int dotIndex = path.LastIndexOf('.');
            if (dotIndex >= 0)
            {
                extension = path.Substring(dotIndex); // Includes the dot
                return path.Substring(0, dotIndex);
            }
            extension = string.Empty;
            return path;
        }
        public static string AsTextAfter(this string source, string split)
        {
            if (source.Contains(split))
            {
                var parts = source.Split(split);
                if (parts.Length > 1)
                    return parts[1];
            }
            return "";
        }
        public static string AsTextBefore(this string source, string split)
        {
            if (source.Contains(split))
            {
                var parts = source.Split(split);
                if (parts.Length > 1)
                    return parts[0];
            }
            return "";
        }
        public static string GetDirectoryPath(this string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path is null or empty.");
                return string.Empty;
            }

            // Check if the path is a directory
            if (Directory.Exists(path))
            {
                return path;
            }

            // Check if the path is a file
            if (File.Exists(path))
            {
                return Path.GetDirectoryName(path);
            }

            Debug.LogError("Invalid path provided.");
            return string.Empty;
        }
        public static string SystemToAssetPath(this string s)
        {
            return ("Assets/" + s.Substring(Application.dataPath.Length + 1)).Replace('\\', '/');
        }
        public static string SuccessOrFail(this bool flag) => flag ? "SUCCESS" : "FAIL";
        public static string NameOrNull(this Type type) => type == null ? "NULL" : type.Name;
        public static string NameOrNull(this Object o) => o ? o.name : "NULL";
        public static string NameOrEmptyString(this Object o) => o ? o.name : "";
        public static string NameOr(this object obj, string alt)
        {
            if (obj == null)
                return alt;

            var result = "";
            var type = obj.GetType();
            if (type.GetMember("Name").Any(m => m.TryGetValue(obj, out result)))
                return result;
            else if (type.GetMember("name").Any(m => m.TryGetValue(obj, out result)))
                return result;
            else
                return alt;
        }
        public static string NameOr(this Object o, object print) => o ? o.name : print.ToStringOrNull();
        public static string NameOrNull(this MethodInfo info) => info == null ? "NULL" : info.Name;
        public static string TypeNameOrNull(this object obj) => obj == null ? "NULL" : obj.GetType().Name;
        public static string ValueStringOrNull(this object obj) => obj == null ? "NULL" : obj.ToString();
        public static string ToStringOrNull(this object str)
        {
            return str == null ? "NULL" : str.ToString();
        }
        public static bool IsEmpty(this string _) => string.IsNullOrWhiteSpace(_);
        public static bool HasContents(this string _) => !_.IsEmpty();
        public static bool IsNotEmpty(this string _) => !_.IsEmpty();
        public static bool HasExtension(this string path, params string[] extensions)
        {
            var a = Path.GetExtension(path).TrimStart('.').ToLower();
            foreach (var _ in extensions)
            {
                var b = _.TrimStart('.').ToLower();
                if (b.Equals(a))
                    return true;
            }
            return false;
        }
        public static bool TryGetTextBetweenChars(this string input, char startChar, char endChar, out string output)
        {
            output = "";
            var result = input.TryGetContainingIndices(startChar.ToString(), endChar.ToString(), out int start_i, out int end_i);

            if (result)
                output = input.Substring(start_i + 1, end_i - start_i - 1);

            // Extract the substring between the characters, excluding the characters themselves
            return result;
        }
        public static bool TryGetContainingIndices(this string input, string before, string after, out int start, out int end)
        {
            start = input.IndexOf(before);
            end = input.IndexOf(after);
            return !(start == -1 || end == -1 || start >= end);
        }
        public static bool TryGetLastContainingIndices(this string input, string before, string after, out int start, out int end)
        {
            start = input.LastIndexOf(before);
            end = input.LastIndexOf(after);
            return 0 <= start && start < end && end < input.Length;
        }
        public static string JoinString<T>(this IEnumerable<T> items, char separator = ',', Func<T, string> getString = null)
        {
            getString ??= _ => _.ToString();
            return string.Join(separator, items.Select(getString));
        }
        public static string JoinString(this IEnumerable<string> items)
        {
            var sb = new StringBuilder();
            foreach (var s in items)
            {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }
        public static string[] AsArray(this string text) => new string[] { text };
        public static string ExtractLast(this string input, string before, string after)
        {
            if (input.TryGetLastContainingIndices(before, after, out int start_i, out int end_i))
            {
                return input.Substring(start_i + 1, end_i - start_i - 1);
            }
            return input;
        }
        #endregion

        #region FANCY_DROPDOWNS
        // use reflection to reach UnityDropdown package because I don't want extra scripts

        static readonly Type DropdownItemType = Type.GetType("UnityDropdown.Editor.DropdownItem`1, UnityDropdown.Editor");
        static readonly Type DropdownMenuType = Type.GetType("UnityDropdown.Editor.DropdownMenu`1, UnityDropdown.Editor");
        static Type GetDropdownItemType<T>() => DropdownItemType.MakeGenericType(typeof(T));
        static Type GetDropdownMenuType<T>() => DropdownMenuType.MakeGenericType(typeof(T));

        public static void ShowDropdown<T>(Action<T> callback, bool sortItems, bool showNoneElement, string selected, params (T context, string text)[] items)
        {
            var itemType = GetDropdownItemType<T>();
            var menuType = GetDropdownMenuType<T>();

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
        public static void ShowDropdown<T>(Action<T> callback, bool sortItems, bool showNoneElement, T selected, params (T context, string text)[] items)
        {
            var itemType = GetDropdownItemType<T>();
            var menuType = GetDropdownMenuType<T>();

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
        public static void ShowDropdown<T>(Action<T> callback, bool sortItems, bool showNoneElement, T selected, params T[] items)
        {
            var itemType = GetDropdownItemType<T>();
            var menuType = GetDropdownMenuType<T>();

            // Create the list of DropdownItems
            var itemList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

            foreach (var t in items)
            {
                var dropdownItem = Activator.CreateInstance(itemType, t.ToString(), t.ToString(), null, null, selected.Equals(t));
                itemList.Add(dropdownItem);
            }

            // Create DropdownMenu with the items list and a callback action
            var dropdownMenu = Activator.CreateInstance(menuType, itemList, callback, 10, sortItems, showNoneElement);
            var showAsContextMethod = menuType.GetMethod("ShowAsContext", BindingFlags.Instance | BindingFlags.Public);
            showAsContextMethod.Invoke(dropdownMenu, new object[] { 0 });
        }

        public static GUIContent GetDropdownContent(this MemberInfo info)
        {

            if (info == null)
                return new GUIContent("(none)");

            var content = new GUIContent($"{info.DeclaringType.Name}.{info.Name}");

            if (info is MethodInfo method)
            {
                content.text = $"{content.text} {string.Join(',', method.GetParameterTypes().Select(t => t.Name))}";
            }
            return content;
        }
        public static string GetFullDropdownContent(this MemberInfo info)
        {
            return GetModuleName(info) + "/" + GetDropdownContent(info);
        }
        public static string GetDropdownPath(this MemberInfo info)
        {
            return info.Module.Name.FileName() + "/";
        }
        public static string GetModuleName(this MemberInfo info)
        {
            return Path.GetFileNameWithoutExtension(info.Module.Name);
        }


        #endregion

        #region DEBUG_EXTENSIONS
        public static void Try(System.Action action, int messageType = 0, UnityEngine.Object link = null)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                if (messageType == 1)
                {
                    if (link) Debug.LogWarning(e.StackTrace, link);
                    else Debug.LogWarning(e.StackTrace);
                }
                else if (messageType == 2)
                {
                    if (link) Debug.LogError(e.StackTrace, link);
                    else Debug.LogError(e.StackTrace);
                }
            }
        }
        public static void Bench(Action a, string mesg = "Action")
        {
            mesg ??= "Action";
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            a.Invoke();
            watch.Stop();

            Debug.Log($"{mesg} took {watch.ElapsedMilliseconds} MS");
        }
        public static string GetExceptionInfo(this Exception e)
        {
            return e.Message.WithHTML(Color.red)
                + "\n\nfrom: " + e.Source
                + "in: " + e.StackTrace;
        }
        public static Exception Log(this Exception e, string text = null, Object context = null)
        {
            var info = e.GetExceptionInfo();
            text = text == null ? e.GetExceptionInfo() : text + "\n" + e.GetExceptionInfo();
            Debug.Log(text, context);
            return e;
        }
        public static Exception Log(this Exception e, Object context)
        {
            var text = e.GetExceptionInfo();
            Debug.Log(text, context);
            return e;
        }
        public static void LogWarning(this Exception e, string text, Object context = null)
        {
            text += e.GetExceptionInfo();
            Debug.LogWarning(text, context);
        }
        public static void LogError(this Exception e, string text, Object context = null)
        {
            text += e.GetExceptionInfo();
            Debug.LogError(text, context);
        }
        public static void Log(this bool b, string text, Object context = null)
        {
            if (b)
            {
                if (context)
                    Debug.Log(text, context);
                else
                    Debug.Log(text);
            }
        }
        public static void Log(string text)
        {
            Debug.Log(text);
        }
        public static void Trace(this object obj)
        {
            var stackTrace = new StackTrace(true); // true to capture file information
            var frames = stackTrace.GetFrames();

            if (frames == null) return;

            var callStack = "";
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                var fileName = frame.GetFileName();
                var lineNumber = frame.GetFileLineNumber();
                callStack += $"{method.DeclaringType.FullName}.{method.Name} in {fileName}:line {lineNumber}\n";
            }

            Debug.Log(callStack);
        }
        #endregion

        #region IDK
        public static string[] GetLayerNames()
        {
            var list = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (!name.IsEmpty())
                    list.Add(name);
            }
            return list.ToArray();
        }
        #endregion

#if UNITY_EDITOR

        #region CONSTANTS
        public static object buffer;
        public static float IndentWidth => 17f;
        public static float FullLineHeight => LineHeight + VerticalSpacing;
        public static float LineHeight => EditorGUIUtility.singleLineHeight;
        public static float LabelWidth => EditorGUIUtility.labelWidth;
        public static float ButtonWidth => 22f;
        public static float VerticalSpacing => EditorGUIUtility.standardVerticalSpacing;
        public static string CopyBuffer => EditorGUIUtility.systemCopyBuffer;
        public static Vector2 screenSize => new Vector2(Screen.width, Screen.height);
        public static GUIContent locked => EditorGUIUtility.IconContent("Locked@2x");
        public static GUIContent unlocked => EditorGUIUtility.IconContent("Unlocked@2x");
        public static GUIContent settings => EditorGUIUtility.IconContent("d_SettingsIcon@2x");
        public static GUIContent select => EditorGUIUtility.IconContent("d_scenepicking_pickable_hover@2x");
        public static GUIContent refresh => EditorGUIUtility.IconContent("d_Refresh@2x");
        public static GUIContent star => EditorGUIUtility.IconContent("d_Favorite On Icon");
        public static GUIContent sceneOut => EditorGUIUtility.IconContent("SceneLoadOut");
        public static GUIContent sceneIn => EditorGUIUtility.IconContent("SceneLoadIn");
        public static GUIContent hidden => EditorGUIUtility.IconContent("scenevis_hidden@2x");
        public static GUIContent visible => EditorGUIUtility.IconContent("d_animationvisibilitytoggleon@2x");
        #endregion

        #region USING_SCOPES
        public class PropertyScope : IDisposable
        {
            public PropertyScope(Rect position, SerializedProperty property, GUIContent label, bool box = true)
            {
                EditorGUI.BeginProperty(position, label, property);
                if (box)
                    IndentBoxGUI(position);
            }
            void IDisposable.Dispose()
            {
                EditorGUI.EndProperty();
            }
        }
        public static PropertyScope Property(Rect position, SerializedProperty property, GUIContent label, bool box = true)
        {
            return new PropertyScope(position, property, label, box);
        }
        public static PropertyScope Property(Rect position, SerializedProperty property, bool box = true)
        {
            return new PropertyScope(position, property, GUIContent.none, box);
        }
        public static PropertyScope Property(Rect position, GUIContent label, SerializedProperty property, bool box = true)
        {
            return new PropertyScope(position, property, label, box);
        }
        public static void IndentBoxGUI(Rect position)
        {
            var color = EditorGUI.indentLevel % 2 == 0 ? NGUI.Shadow : NGUI.Light;
            EditorGUI.DrawRect(EditorGUI.IndentedRect(position), color);
        }
        public class DisabledScope : IDisposable
        {
            public DisabledScope(bool disabled = true) => EditorGUI.BeginDisabledGroup(disabled);
            public void Dispose() => EditorGUI.EndDisabledGroup();
        }
        public static DisabledScope Disabled(bool disabled = true)
        {
            return new DisabledScope(disabled);
        }
        #endregion

        #region REFLECTION
        public static bool HasPropertyDrawer(this Type targetType)
        {
            drawerTypes ??= GetAllTypesWithPropertyDrawer();

            return drawerTypes.ContainsKey(targetType);
        }
        public static IEnumerable<Type> GetAssignableReferenceTypes(this Type fieldType, List<Func<Type, bool>> filters = null)
        {
            var appropriateTypes = new List<Type>();

            try
            {
                if (fieldType != null)
                {

                    // Get and filter all appropriate types
                    foreach (var type in TypeCache.GetTypesDerivedFrom(fieldType))
                    {
                        if (!type.IsActivatable())
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
                if (fieldType.IsActivatable())
                    appropriateTypes.Insert(0, fieldType);
            }


            return appropriateTypes;
        }
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
                parentProperty = NGUI.Parent(parentProperty);
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
                return targetObject;

            // If not found, traverse parent properties' managedReferenceValues
            SerializedProperty parentProperty = property.Copy();
            while (parentProperty.depth > 0)
            {
                parentProperty = NGUI.Parent(parentProperty);
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

        static Dictionary<Type, Type> drawerTypes;
        public static Dictionary<Type, Type> GetAllTypesWithPropertyDrawer()
        {
            var result = new Dictionary<Type, Type>();
            var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var drawerType in TypeCache.GetTypesDerivedFrom<PropertyDrawer>())
            {
                var attributes = drawerType.GetCustomAttributes(typeof(CustomPropertyDrawer), true)
                                           .Cast<CustomPropertyDrawer>();

                foreach (var attr in attributes)
                {
                    var targetType = (Type)typeField.GetValue(attr);
                    if (!result.ContainsKey(targetType))
                        result.Add(targetType, drawerType);
                }
            }

            return result;
        }
        public static void CopyMatchingFields(object target, object source)
        {
            if (source == null || target == null)
                return;

            var targetType = target.GetType();
            var sourceType = source.GetType();

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.FlattenHierarchy;

            var sourceFields = sourceType.GetFields(flags);
            foreach (var sourceField in sourceFields)
            {
                try
                {
                    var targetField = targetType.GetField(sourceField.Name);
                    targetField.SetValue(target, sourceField.GetValue(source));
                }
                catch // (Exception e)
                {
                    // target does not contain source field
                    //Debug.LogWarning($"Could not copy property {sourceField.Name}", (UnityEngine.Object)reference);
                }
            }
        }

        public static TextAsset FindScriptAsset(this MethodInfo method)
        {
            if (method == null)
                throw new System.ArgumentException("f u");

            // Get file path using Debug symbols
            // Generate a StackFrame to retrieve file and line info
            var stackTrace = new StackTrace(true);
            foreach (var frame in stackTrace.GetFrames())
            {
                if (frame.GetMethod() == method)
                {
                    string filePath = frame.GetFileName();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var assetPath = SystemToAssetPath(filePath);
                        return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    }
                }
            }
            return null;
        }
        public static TextAsset FindScript(this Action action)
        {
            return FindScriptAsset(action.Method);
        }
        public static TextAsset FindScript(this Type type)
        {
            if (type == null)
                throw new System.ArgumentException("f u");

            var stackTrace = new StackTrace(true);
            foreach (var frame in stackTrace.GetFrames())
            {
                if (frame.GetType() == type)
                {
                    string filePath = frame.GetFileName();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        var assetPath = SystemToAssetPath(filePath);
                        return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    }
                }
            }
            return null;
        }

        #endregion

        #region ASSET_DATABASE
        public static IEnumerable<T> LoadAssets<T>(string include = null) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            var paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid));

            if (!include.IsEmpty())
                paths = paths.Where(path => path.Contains(include));

            return paths.Select(path => AssetDatabase.LoadAssetAtPath<T>(path))
                .Where(asset => asset);
        }
        public static string GetAssetPath(this Object _) => AssetDatabase.GetAssetPath(_);
        public static T LoadAssetFromGUID<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
        public static T LoadFirstAsset<T>(string search) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets(search);
            if (guids.Length == 0)
                return default;
            return LoadAssetFromGUID<T>(guids[0]);
        }
        public static bool TryFindFirstAsset(string search, out Object result)
        {
            var guids = AssetDatabase.FindAssets(search);
            if (guids.Length == 0)
            {
                result = null;
                return false;
            }

            var guid = guids[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return result = AssetDatabase.LoadAssetAtPath<Object>(path);
        }
        public static List<T> FindAssetsOfType<T>() where T : Object
        {
            return FindAssetsOfType(typeof(T)).ConvertAll(o => (T)o);
        }
        public static List<Object> FindAssetsOfType(Type type)
        {
            if (!typeof(Object).IsAssignableFrom(type))
            {
                Debug.LogError($"Type {type.Name} does not derive from UnityEngine.Object");
                return new List<Object>();
            }

            List<Object> assets = new List<Object>();

            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
        public static IEnumerable<T> FindAssets<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(path => AssetDatabase.LoadAssetAtPath<T>(path));
        }
        public static void Dirty(this Object obj)
        {
#if UNITY_EDITOR
            // EditorUtility.CopySerializedManagedFieldsOnly
            Undo.RecordObject(obj, obj.name);
            EditorUtility.SetDirty(obj);
#endif
        }
        public static bool FindAsset<T>(string search, out T result) where T : Object
        {
            var guids = AssetDatabase.FindAssets(search);
            if (guids.Length == 0)
            {
                result = null;
                return false;
            }

            var guid = guids[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return result = AssetDatabase.LoadAssetAtPath<T>(path);
        }
        public static void CreateAssetDialogue<T>(string name = null) where T : ScriptableObject
        {
            // Prompt the user to choose a save location and file name
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Asset",
                name ?? $"New{typeof(T).Name}",
                "asset",
                "Please enter a file name to save the asset to"
            );

            // Check if the user canceled the save file dialogue
            if (string.IsNullOrEmpty(path))
                return;

            // Create the asset and save it to the chosen path
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            // Focus the Project window and highlight the newly created asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        public static T CreateAsset<T>() where T : ScriptableObject
        {
            // try to find the selected asset's folder
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(selectedPath))
                selectedPath = System.IO.Path.GetDirectoryName(selectedPath);

            // default to Assets/
            if (!AssetDatabase.IsValidFolder(selectedPath))
                selectedPath = Application.dataPath;

            // instantiate into AssetDatabase
            T instance = ScriptableObject.CreateInstance<T>();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{selectedPath}/{typeof(T).Name}.asset");
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = instance;
            EditorGUIUtility.PingObject(instance);
            return instance;
        }
        public static T CreateAsset<T>(T from, string path = null) where T : ScriptableObject
        {
            // Get the selected object in the Project window
            var selectedObject = Selection.activeObject;

            var x = ScriptableObject.Instantiate(from);

            // Get the path of the selected object
            path ??= Application.dataPath;

            if (selectedObject)
            {
                // If the selected object is not a folder, select its parent folder
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                }
            }
            else
            {
                var script = MonoScript.FromScriptableObject(x);
                path = AssetDatabase.GetAssetPath(script);
                path = Path.GetDirectoryName(path);
                path = path.Replace('\\', '/') + "/";
            }

            // Base asset folder if no path selected
            if (!AssetDatabase.IsValidFolder(path))
            {
                path = Application.dataPath;
                path = path.Substring(path.Length - "Assets".Length);
            }

            // Create a unique asset file name within the selected folder
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{typeof(T).Name}.asset");

            // Create the asset in the Project window
            AssetDatabase.CreateAsset(x, assetPath);

            // Save the asset to disk
            AssetDatabase.SaveAssets();

            // Select the newly created asset
            Selection.activeObject = x;
            EditorGUIUtility.PingObject(x);

            return x;
        }
        public static T CreateUniqueAsset<T>(T obj) where T : Object
        {
            var oldPath = AssetDatabase.GetAssetPath(obj);
            var newPath = AssetDatabase.GenerateUniqueAssetPath(oldPath);

            File.Copy(oldPath, newPath);
            AssetDatabase.Refresh();
            T clone = AssetDatabase.LoadAssetAtPath<T>(newPath);
            clone.name = Path.GetFileNameWithoutExtension(newPath);
            Undo.RegisterCreatedObjectUndo(clone, "Create asset");
            Undo.undoRedoPerformed += () =>
            {
                if (!AssetDatabase.Contains(clone))
                {
                    //Editor.DestroyImmediate(clone);
                    File.Delete(newPath);
                    File.Delete(newPath + ".meta");
                    AssetDatabase.Refresh();
                }
            };
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(clone);
            AssetDatabase.Refresh();
            return clone;
        }
        #endregion

        #region RECT_CONTENT_EXTENSIONS
        public static Rect NextLine(this Rect rect, float? height = null)
        {
            return rect.With(y: rect.yMax + NGUI.VerticalSpacing, h: height.HasValue ? height.Value : NGUI.LineHeight);
        }
        public static Rect GetRect()
        {
            //return GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, FullLineHeight).With(h: LineHeight);
            return GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, LineHeight);  // does the vertical spacing need to be reserved
        }
        public static Rect With(this Rect rect, Vector2? pos = null, Vector2? size = null, float? x = null, float? y = null, float? w = null, float? h = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
        {
            if (pos.HasValue)
            {
                rect.position = pos.Value;
            }
            if (size.HasValue)
            {
                rect.size = size.Value;
            }
            if (x.HasValue)
            {
                rect.x = x.Value;
            }
            if (y.HasValue)
            {
                rect.y = y.Value;
            }
            if (w.HasValue)
            {
                rect.width = w.Value;
            }
            if (h.HasValue)
            {
                rect.height = h.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin = xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax = xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin = yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax = yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min = min.Value;
            }
            if (max.HasValue)
            {
                rect.max = max.Value;
            }

            return rect;
        }
        public static RectInt With(this RectInt rect, Vector2Int? pos = null, Vector2Int? size = null, int? x = null, int? y = null, int? w = null, int? h = null, int? xMin = null, int? xMax = null, int? yMin = null, int? yMax = null, Vector2Int? min = null, Vector2Int? max = null)
        {
            if (pos.HasValue)
            {
                rect.position = pos.Value;
            }
            if (size.HasValue)
            {
                rect.size = size.Value;
            }
            if (x.HasValue)
            {
                rect.x = x.Value;
            }
            if (y.HasValue)
            {
                rect.y = y.Value;
            }
            if (w.HasValue)
            {
                rect.width = w.Value;
            }
            if (h.HasValue)
            {
                rect.height = h.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin = xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax = xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin = yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax = yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min = min.Value;
            }
            if (max.HasValue)
            {
                rect.max = max.Value;
            }

            return rect;
        }
        public static Rect Move(this Rect rect, Vector2? position = null, Vector2? size = null, float? x = null, float? y = null, float? w = null, float? h = null, float? xMin = null, float? xMax = null, float? yMin = null, float? yMax = null, Vector2? min = null, Vector2? max = null)
        {
            if (y is float Y)
            {

                var p = rect.position;
                p.y += Y;
                rect.position = p;
            }
            if (x is float X)
            {

                var p = rect.position;
                p.x += X;
                rect.position = p;
            }

            if (position.HasValue)
            {
                rect.position += position.Value;
            }
            if (size.HasValue)
            {
                rect.size += size.Value;
            }
            if (w.HasValue)
            {
                rect.width += w.Value;
            }
            if (h.HasValue)
            {
                rect.height += h.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin += xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax += xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin += yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax += yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min += min.Value;
            }
            if (max.HasValue)
            {
                rect.max += max.Value;
            }

            return rect;
        }
        public static RectInt Move(this RectInt rect, Vector2Int? position = null, Vector2Int? size = null, int? x = null, int? y = null, int? width = null, int? height = null, int? xMin = null, int? xMax = null, int? yMin = null, int? yMax = null, Vector2Int? min = null, Vector2Int? max = null)
        {
            if (y.HasValue)
            {

                var p = rect.position;
                p.y += (int)y;
                rect.position = p;
            }
            if (x.HasValue)
            {

                var p = rect.position;
                p.x += (int)x;
                rect.position = p;
            }

            if (position.HasValue)
            {
                rect.position += position.Value;
            }
            if (size.HasValue)
            {
                rect.size += size.Value;
            }
            if (width.HasValue)
            {
                rect.width += width.Value;
            }
            if (height.HasValue)
            {
                rect.height += height.Value;
            }
            if (xMin.HasValue)
            {
                rect.xMin += xMin.Value;
            }
            if (xMax.HasValue)
            {
                rect.xMax += xMax.Value;
            }
            if (yMin.HasValue)
            {
                rect.yMin += yMin.Value;
            }
            if (yMax.HasValue)
            {
                rect.yMax += yMax.Value;
            }
            if (min.HasValue)
            {
                rect.min += min.Value;
            }
            if (max.HasValue)
            {
                rect.max += max.Value;
            }

            return rect;
        }
        public static GUIStyle With(this GUIStyle style, int? fontSize = null, TextAnchor? alignment = null, float? fixedHeight = null, float? fixedWidth = null, RectOffset padding = null, RectOffset margins = null, Font font = null, bool? richText = null, bool? wordWrap = null, GUIStyleState normal = null, GUIStyleState hover = null, GUIStyleState active = null)
        {
            return new GUIStyle(style)
            {
                fontSize = fontSize ?? style.fontSize,
                alignment = alignment ?? style.alignment,
                fixedHeight = fixedHeight ?? style.fixedHeight,
                fixedWidth = fixedWidth ?? style.fixedWidth,
                margin = margins ?? style.margin,
                padding = padding ?? style.padding,
                font = font ?? style.font,
                richText = richText ?? style.richText,
                wordWrap = wordWrap ?? style.wordWrap,
                normal = normal ?? style.normal,
                hover = hover ?? style.hover,
                active = active ?? style.active,
            };
        }
        public static GUIStyleState With(this GUIStyleState src, Texture2D background = null, Texture2D[] scaledBackground = null, Color? textColor = null)
        {
            return new GUIStyleState()
            {
                background = background ?? src.background,
                scaledBackgrounds = scaledBackground ?? src.scaledBackgrounds,
                textColor = textColor ?? src.textColor,
            };
        }
        public static GUIContent With(this GUIContent content, string text = null, Texture image = null, string tooltip = null)
        {
            content = new GUIContent(content);
            if (text != null)
                content.text = text;
            if (image != null)
                content.image = image;
            if (tooltip != null)
                content.tooltip = tooltip;
            return content;
        }
        #endregion

        #region EVENTS
        public static bool ToolbarButton(GUIContent content)
        {
            return GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(25));
        }
        public static void StartDrag(Object[] objects)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = objects;
            DragAndDrop.StartDrag("Dragging Prefab");
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
        public static bool AcceptDrag(Rect dropArea)
        {
            Event e = Event.current;
            GUI.Box(dropArea, "");

            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:

                    if (!dropArea.Contains(e.mousePosition))
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        return true;
                    }
                    e.Use();
                    break;
            }
            return false;
        }
        public static int MouseDown(Rect rect)
        {
            var e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                if (MouseInRect(rect))
                    return e.button;
            }
            return -1;
        }
        public static bool MouseInRect(Rect rect)
        {
            var mouseLocal = Event.current.mousePosition - rect.position;
            return mouseLocal.x > 0
                && mouseLocal.x < rect.width
                && mouseLocal.y > 0
                && mouseLocal.y < rect.height;
        }
        public static bool MouseUp(this Event Event)
        {
            return Event.type == EventType.MouseUp;// == EventType.MouseUp;// && Event.button == button;
        }
        #endregion

        #region PROPERTIES
        public static Dictionary<Type, Type> FindPropertyDrawerTypes()
        {
            var result = new Dictionary<Type, Type>();
            var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var drawerType in TypeCache.GetTypesDerivedFrom<PropertyDrawer>())
            {
                var attributes = drawerType.GetCustomAttributes(typeof(CustomPropertyDrawer), true)
                                           .Cast<CustomPropertyDrawer>();

                foreach (var attr in attributes)
                {
                    var targetType = (Type)typeField.GetValue(attr);
                    if (!result.ContainsKey(targetType))
                        result.Add(targetType, drawerType);
                }
            }

            return result;
        }
        public static void ContextMenu(SerializedProperty property, Rect rect)
        {
            Event evt = Event.current;

            if (evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                if (property.propertyType == SerializedPropertyType.ManagedReference)
                {
                    menu.AddItem(new GUIContent("Copy managedReferenceValue"), false, () => CopyManagedReferenceValue(property));
                    if (buffer != null && GetManagedReferenceFieldType(property).IsAssignableFrom(buffer.GetType()))
                        //if (buffer != null && Type.GetType(property.managedReferenceFullTypename).IsAssignableFrom(buffer.GetType()))
                        menu.AddItem(new GUIContent("Paste managedReferenceValue"), false, () => PasteManagedReferenceValue(property));
                }
                menu.ShowAsContext();
                evt.Use(); // Mark the event as used
            }
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
        /// <summary> Get the real field type from a SerializeReference field </summary>
        public static Type GetManagedReferenceFieldType(this SerializedProperty property)
        {
            var strings = property.managedReferenceFieldTypename.Split(char.Parse(" "));
            var type = Type.GetType($"{strings[1]}, {strings[0]}");

            if (type == null)
                Debug.LogError($"Can not get field type of managed reference : {property.managedReferenceFieldTypename}");

            return type;
        }
        public static void DrawScriptField(ScriptableObject obj)
        {
            var script = MonoScript.FromScriptableObject(obj);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
        }
        public static void DrawScriptField(SerializedObject obj)
        {
            var target = obj.targetObject;
            var type = obj.targetObject.GetType();

            MonoScript script;
            if (target is ScriptableObject so)
                script = MonoScript.FromScriptableObject(so);
            else if (target is MonoBehaviour mb)
                script = MonoScript.FromMonoBehaviour(mb);
            else
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(script, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
        }
        public static void DrawPropertiesLayout(this SerializedObject serializedObject, params string[] props)
        {
            EditorGUI.BeginChangeCheck();
            foreach (var propName in props)
            {
                var serializedProperty = serializedObject.FindProperty(propName);
                EditorGUILayout.PropertyField(serializedProperty);
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }
        public static void DrawProperties(this SerializedObject serializedObject, Rect rect, params string[] props)
        {
            EditorGUI.BeginChangeCheck();
            foreach (var propName in props)
            {
                var property = serializedObject.FindProperty(propName);
                rect.height = EditorGUI.GetPropertyHeight(property);
                EditorGUI.PropertyField(rect, property);
                rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }
        public static void ApplyAndMarkDirty(this SerializedProperty property)
        {
            //Undo.RecordObject(property.serializedObject.targetObject, "Apply properties");
            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }
        public static bool GetArrayElementIndex(this SerializedProperty property, out int result)
        {
            result = -1;
            if (property.Parent() is SerializedProperty parent && parent.isArray)
            {
                for (int i = 0; i < parent.arraySize; i++)
                {
                    if (parent.GetArrayElementAtIndex(i).propertyPath.Equals(property.propertyPath))
                    {
                        result = i;
                        return true;
                    }
                }
            }
            return false;
        }
        public static SerializedProperty Parent(this SerializedProperty property)
        {
            // https://gist.github.com/monry/9de7009689cbc5050c652bcaaaa11daa
            var propertyPaths = property.propertyPath.Split('.');
            if (propertyPaths.Length <= 1)
            {
                return default;
            }

            var parentSerializedProperty = property.serializedObject.FindProperty(propertyPaths.First());
            for (var index = 1; index < propertyPaths.Length - 1; index++)
            {
                if (propertyPaths[index] == "Array" && propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                {
                    var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                    var arrayIndex = int.Parse(match.Groups[1].Value);
                    parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                    index++;
                }
                else
                {
                    parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
                }
            }

            return parentSerializedProperty;
        }
        public static SerializedProperty FindSiblingProperty(this SerializedProperty property, string siblingPropertyName)
        {
            if (property == null)
                throw new System.ArgumentNullException(nameof(property));

            if (string.IsNullOrEmpty(siblingPropertyName))
                throw new System.ArgumentException("Sibling property name cannot be null or empty.", nameof(siblingPropertyName));

            // Get the property path of the current property
            string propertyPath = property.propertyPath;

            // Find the last separator in the property path
            int lastSeparator = propertyPath.LastIndexOf('.');

            // Determine the parent path
            string parentPath = lastSeparator >= 0 ? propertyPath.Substring(0, lastSeparator) : "";

            // Construct the sibling's property path
            string siblingPropertyPath = string.IsNullOrEmpty(parentPath) ? siblingPropertyName : $"{parentPath}.{siblingPropertyName}";

            // Find and return the sibling property
            return property.serializedObject.FindProperty(siblingPropertyPath);
        }
        public static void DrawProperties(this SerializedProperty property, Rect position)
        {
            var currentProperty = property.Copy();
            var endProperty = property.GetEndProperty();

            bool enterChildren = true;

            while (currentProperty.NextVisible(enterChildren))
            {
                if (SerializedProperty.EqualContents(currentProperty, endProperty))
                    break;

                position = position.With(h: currentProperty.GetHeight());
                EditorGUI.PropertyField(position, currentProperty, true);
                position.y += position.height + NGUI.VerticalSpacing;
                enterChildren = false;
            }
        }
        public static void DrawAllProperties(this SerializedObject serializedObject, Rect rect)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            float currentY = rect.y;

            while (property.NextVisible(enterChildren))
            {
                EditorGUI.BeginDisabledGroup(property.displayName == "Script");

                enterChildren = false;
                EditorGUI.PropertyField(
                    new Rect(rect.x, currentY, rect.width, EditorGUI.GetPropertyHeight(property, true)),
                    property,
                    true
                );
                currentY += EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.EndDisabledGroup();
            }
        }
        public static float GetCumulativeHeight(this SerializedObject serializedObject)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            float totalHeight = 0f;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                totalHeight += EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }
        public static bool HasChildProperties(this SerializedProperty property)
        {
            //Debug.Log(property.propertyPath + " " + property.Copy().CountRemaining());
            var copy = property.Copy();
            var flag = copy.isExpanded;
            copy.isExpanded = true;
            var count = copy.CountInProperty() > 1;
            property.isExpanded = flag;
            return count;
        }
        public static bool IsArrayElement(this SerializedProperty property)
        {
            return property.propertyPath.EndsWith(']');
        }
        public static bool IsArrayElement(this SerializedProperty property, out int index)
        {
            index = -1;
            var result = property.IsArrayElement();

            if (result)
            {
                var start = property.propertyPath.LastIndexOf('[') + 1;
                var str = property.propertyPath.Substring(start, (property.propertyPath.Length - 1) - start);
                index = int.Parse(str);
            }
            return result;
        }
        public static float GetHeight(this SerializedProperty property) => EditorGUI.GetPropertyHeight(property);
        /// <summary>Easy-mode property.isExpanded=EditorGUI.Foldout...etc...</summary>
        public static bool IsExpanded(this SerializedProperty property, Rect position, GUIContent label = null)
        {
            label ??= GUIContent.none;
            return property.isExpanded = EditorGUI.Foldout(position.With(h: LineHeight), property.isExpanded, label, true);
        }
        public static bool GetArrayProperty(this SerializedProperty property, out SerializedProperty arrayProperty)
        {
            var i = property.propertyPath.LastIndexOf(".Array");
            var newPath = property.propertyPath.Substring(0, i);
            arrayProperty = property.serializedObject.FindProperty(newPath);

            return arrayProperty != null;
        }
        public static bool IsArrayOrElement(this SerializedProperty property)
        {
            return property.isArray || IsElement(property);
        }
        public static bool IsElement(this SerializedProperty property)
        {
            return property.propertyPath.EndsWith(']');
        }
        #endregion

#endif
    }
}