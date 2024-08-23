using System.Text;
using UnityEngine;
using System;

namespace Neeto
{
    [Serializable]
    public struct SortingLayerGenerator : IScriptGenerator
    {
        public string Script()
        {
            var sb = new StringBuilder("public static class SortingLibrary\n{");
            foreach (var layer in SortingLayer.layers)
            {
                sb.AppendLine($"\tpublic const int {layer.name.Replace(' ', '_')} = {layer.value};");
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}