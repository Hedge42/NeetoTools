using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Neeto
{
    [Serializable]
    public struct ResourceTextureGenerator : IScriptGenerator
    {
        public string Script()
        {
            var sb = new StringBuilder();
            var textures = Resources.FindObjectsOfTypeAll<Texture2D>();

            foreach (var t in textures)
            {
                var path = AssetDatabase.GetAssetPath(t);
                sb.AppendLine(t.name);
            }

            return sb.ToString();
        }
    }
}