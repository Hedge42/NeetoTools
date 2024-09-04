using UnityEngine;
using System.IO;
using System;

[Serializable]
public partial struct MenuPath
{
    public string content;

    public static implicit operator string(MenuPath _) => _.content;
    public static implicit operator MenuPath(string _) => new MenuPath { content = _ };
    public MenuPath(string content) => this.content = content;


    public const string Neeto = "Neeto (ง'̀-'́)ง/";
    public const string Events = Neeto + nameof(Events) + "/";
    public const string Run = Neeto + "Run/";
    public const string Open = Neeto + "Open/";
    public const string Tools = Neeto + "Tools/";
    public const string Debug = Neeto + "Debug/";
    public const string Animation = Neeto + "Animation/";
    public const string Dialogue = Neeto + "Dialogue/";
    public const string Var = Neeto + "Var";
    public const string Assets = "Assets/" + Neeto;
    public const string GameObject = "GameObject/" + Neeto;
}

