using Matchwork;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class MLayer
{
    #region GENERATED
		public const int Default = 1;
		public const int TransparentFX = 2;
		public const int Ignore_Raycast = 4;
		public const int Camera = 8;
		public const int Water = 16;
		public const int UI = 32;
		public const int Level = 64;
		public const int Player = 128;
		public const int Enemy = 256;
		public const int Terrain = 512;
		public const int Weapon = 1024;
		public const int Ragdoll = 2048;
		public const int Interactable = 4096;
		public const int Special = 8192;
		public const int Trigger = 16384;
		public const int Projectile = 32768;
		public const int PlayerTrigger = 65536;
		public const int CharacterTrigger = 131072;
		public const int PlayerHitbox = 262144;
		public const int EnemyHitbox = 524288;
#endregion

    #region Editor
#if UNITY_EDITOR
    [MenuItem(MPath.Commands + "Generate Physics Layers")]
    static void Generate()
    {
        var asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Gylfie/Scripts/Extensions/MLayer.cs");
        StringBuilder sb = new();
        for (int i = 0; i < 32; i++)
        {
            var name = LayerMask.LayerToName(i);
            if (name.IsEmpty())
                continue;

            sb.AppendLine($"\t\tpublic const int {name.Replace(' ', '_')} = {1 << i};");
        }
        Assets.ExportRegion(asset, "GENERATED", sb.ToString());
    }
#endif
    #endregion

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

    public static readonly LayerMask Environment = MLayer.Level | MLayer.Terrain;
    public static readonly int Walkable = 1;
    public static void SetLayer(this Collider[] colliders, int layer)
    {
        foreach (var collider in colliders)
        {
            if (collider.gameObject.layer != layer)
                collider.gameObject.layer = layer;
        }
    }
    public static bool Evaluate(this LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
    public static bool Evaluate(this LayerMask mask, GameObject gameObject)
    {
        return mask.Evaluate(gameObject.layer);
    }
    public static bool Evaluate(this LayerMask mask, Collider collider)
    {
        return mask.Evaluate(collider.gameObject.layer);
    }
}