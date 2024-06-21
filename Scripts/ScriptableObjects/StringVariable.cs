using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Variables/" + nameof(StringVariable))]
//[MonoScript(typeof(FloatVariable))]
public class StringVariable : Variable<string> { }


public interface IVariable { object referenceValue { get; } }
public interface IVariable<T> : IVariable { T Value { get; } }


[Serializable]
public class IStringVariable : IVariable<string>
{
    public string text;
    public string Value => text;
    public object referenceValue => text;
}

[Serializable]
public class DocTemplate : IVariable<string>
{
    public string text = "<size=24><b>TITLE:</b> What this is about</size>\n"
                       + "<size=18><b>DESCRIPTION:</b> What this does</size>\n\n"
                       + "<size=18><b>LINKS:</b>\n"
                       + "<color=blue><link=\"Drive\">Click [this]</link> to access Drive assets</color>\n"
                       + "<color=blue><link=\"GitHub\">Click [this]</link> link to view on GitHub</color></size>";

    // To use this string, you would render it in a GUI element that supports rich text.

    public string Value => text;
    public object referenceValue => text;
}