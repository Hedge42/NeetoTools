using UnityEngine;
using System.IO;

public static class MPath
{
    public static string persistent => Application.persistentDataPath;
    public static string project => Path.GetDirectoryName(Application.dataPath) + '\\';

#if UNITY_EDITOR
    public const int TOP = 9999;
    public const int BOTTOM = -9999;

    public const string wtf = "(ง'̀-'́)ง";
    public const string meh = "¯\\_(ツ)_/¯";
    public const string Main = "( ͡° ͜ʖ ͡°) /";
    public const string Dialogue = Main + "Dialogue/";
    public const string Object = Main + "ScriptableObject/";
    public const string Events = Main + "Events/";
    public const string Var = Main + "Variables/";
    public const string Animation = Main + "Animation/";
    public const string Debug = Main + "Debug/";
    public const string Utils = Main + "Utils/";
    public const string Create = Main + "Create/";
    public const string Tools = Main + "Tools/";
    public const string Commands = Main + "Commands/";
    public const string Context = "Object/";
    public const string Assets = "Assets/" + Main;
    public const string GameObject = "GameObject/" + Main;
    public const string CreateAsset = "Assets/Create/" + Main;


    public static string AssetToSystem(string assetPath)
    {
        var dataPath = Application.dataPath;
        dataPath = dataPath.Replace('\\', '/').Substring(0, dataPath.Length - "Assets".Length);
        return Path.Combine(dataPath, assetPath);
    }
#endif
}
