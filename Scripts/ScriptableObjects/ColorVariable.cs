using UnityEditor;
using UnityEngine;

public class ColorVariable : ScriptableObject
{
    public static implicit operator Color(ColorVariable obj) => obj.color;

    [ColorUsage(true, true)]
    public Color color;

#if UNITY_EDITOR
    [MenuItem(MPath.Var + "Color Variable")]
    public static void CreateAsset()
    {
        MEdit.CreateAsset<ColorVariable>();
    }
#endif
}
