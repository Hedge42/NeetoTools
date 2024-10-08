using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Neeto
{
    public static class LayerLibrary
    {
        [QuickAction]
        public static void Script()
        {
            var script = ScriptGenerator.FindScript(typeof(LayerLibrary));
            
            // sorting layers
            var sb = new StringBuilder();
            foreach (var layer in SortingLayer.layers)
            {
                sb.AppendLine($"\t\t\tpublic const int {ScriptGenerator.Codify(layer.name)} = {layer.value};");
            }
            ScriptGenerator.OverwriteRegion(script, "SortingLayers", sb.ToString());

            // physics layers
            sb.Clear();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (name.IsEmpty())
                    continue;

                sb.AppendLine($"\t\t\tpublic const int {ScriptGenerator.Codify(name)} = {1 << i};");
            }
            ScriptGenerator.OverwriteRegion(script, "PhysicsLayers", sb.ToString());
        }

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

        public static readonly int Walkable = 1;
        public static LayerMask Environment => PhysicsLayers.Level | PhysicsLayers.Terrain;

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

        public static class SortingLayers
        {
            #region SortingLayers
	public const int Default = 0;
#endregion SortingLayers
        }
        public static class PhysicsLayers
        {
            #region PhysicsLayers
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
#endregion PhysicsLayers
        }
        public static class NavLayers
        {
            #region NavLayers
            public const int Default = 0;
            #endregion NavLayers
        }
    }
}