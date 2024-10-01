using UnityEngine;
using System;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

public static class NString
{
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
    public static string SuccessOrFail(this bool flag) => flag ? "SUCCESS" : "FAIL";
    public static string[] AsArray(this string text) => new string[] { text };
    /// <summary>SUS I don't think this works</summary>
    public static string AsColor(this string text, string color)
    {
        return $"<color={color}>{text}</color>";
    }
    public static string AsColor(this string text, Color color)
    {
        return $"<color={color.AsHex()}>{text}</color>";
    }
    public static string SystemToAssetPath(this string s)
    {
        var path = Application.dataPath;
        s = s.Substring(path.Length);
        return "Assets/" + s;
    }
    public static string NameOrNull(this Type type) => type == null ? "NULL" : type.Name;
    public static string NameOrNull(this Object o) => o ? o.name : "NULL";
    public static string NameOrEmptyString(this Object o) => o ? o.name : "";
    public static string NameOr(this Object o, object print) => o ? o.name : print.ToStringOrNull();
    public static string NameOrNull(this MethodInfo info) => info == null ? "NULL" : info.Name;
    public static string TypeNameOrNull(this object obj) => obj == null ? "NULL" : obj.GetType().Name;
    public static string ValueStringOrNull(this object obj) => obj == null ? "NULL" : obj.ToString();
    public static string ToStringOrNull(this object str)
    {
        return str == null ? "NULL" : str.ToString();
    }
    public static bool IsEmpty(this string _) => _ == null || _.Trim().Length == 0;
    public static bool IsEmptyOrWhiteSpace(this string _) => _.Trim().Length == 0;
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
    public static bool HasContents(this string _) => _.Trim().Length > 0;
    public static bool TryGetTextBetweenChars(this string input, char startChar, char endChar, out string output)
    {
        output = "";
        var result = input.TryGetContainingIndices(startChar.ToString(), endChar.ToString(), out int start_i, out int end_i);

        if (result)
            output = input.Substring(start_i + 1, end_i - start_i - 1);

        // Extract the substring between the characters, excluding the characters themselves
        return result;
    }
    public static string JoinString<T>(this IEnumerable<T> items, char separator = ',', Func<T, string> getString = null)
    {
        getString ??= _ => _.ToString();
        return string.Join(separator, items.Select(getString));
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
    public static string ExtractLast(this string input, string before, string after)
    {
        if (input.TryGetLastContainingIndices(before, after, out int start_i, out int end_i))
        {
            return input.Substring(start_i + 1, end_i - start_i - 1);
        }
        return input;
    }
}
