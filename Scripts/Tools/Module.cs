//using Cysharp.Threading.Tasks;
using System.Linq;
using System;
using UnityEditor;
using Debug = UnityEngine.Debug;
using Assembly = System.Reflection.Assembly;

namespace Neeto
{

    [Serializable]
    public struct Module
    {
        public string name;

        public const char SEP = ';';
        public static implicit operator Module(string assemblyName) => new Module { name = assemblyName };
        public static implicit operator string(Module pkg) => pkg.name;
        public static implicit operator Module(Module[] pkg) => string.Join(SEP, pkg);
        public Module(string asm) => name = asm;
        public Module(params string[] list) => name = string.Join(SEP, list);
        public Assembly load => Assembly.Load(name);
        public static Module ALL => new Module(NeetoSettings.instance.reflectionScope.JoinString(';', a => a.name));

        public override string ToString() => name;
        string[] split => name.Split(SEP);
        public Module Combine(Module other) => string.Join(SEP, split.Union(other.split));

        public Type GetType(string typeName)
        {
            if (typeName.IsEmpty())
                return null;
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in split.Select(Assembly.Load))
            {
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            if (type == null && !string.IsNullOrWhiteSpace(typeName))
                Debug.LogWarning($"{typeName} not found");

            return null;
        }
        public Type[] GetTypes()
        {
            return split.Select(Assembly.Load)
                             .SelectMany(_ => _.GetTypes())
                             .ToArray();
        }
    }

    [Serializable]
    public class ModuleReference
    {
        public Module module;

#if UNITY_EDITOR
        [GUIChanged(nameof(OnAssetChanged), true)]
        public UnityEditorInternal.AssemblyDefinitionAsset asset;
#endif

        void OnAssetChanged()
        {
            Debug.Log(module.name = asset.NameOrNull());
        }
    }
}