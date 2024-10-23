using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net.Http.Headers;

namespace Neeto
{
    public static class StringHelper
    {
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
            var path = Application.dataPath;
            s = s.Substring(path.Length);
            return "Assets/" + s;
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
        public static string[] AsArray(this string text) => new string[] { text };
        public static string ExtractLast(this string input, string before, string after)
        {
            if (input.TryGetLastContainingIndices(before, after, out int start_i, out int end_i))
            {
                return input.Substring(start_i + 1, end_i - start_i - 1);
            }
            return input;
        }
    }
}