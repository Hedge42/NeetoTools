using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using static System.Reflection.BindingFlags;

namespace Neeto
{
    [CreateAssetMenu(menuName = Menu.Main + nameof(ScriptableAction))]
    public class ScriptableAction : ScriptableObject, IScriptedAction
    {
        [NoFoldout] public ScriptedAction action;
        public void Perform() => action.Perform();
    }

    public interface IScriptedAction
    {
        void Perform();
    }
    [Serializable]
    public class ScriptedAction : IScriptedAction
    {
        public BindingFlags flags = Public | Static | OptionalParamBinding;
        public string excludeAssemblyNames;
        public string excludeTypeNames;

        [ReflectionFilter(nameof(DoReflection))]
        public GameAction action;

        public IEnumerable<MemberInfo> DoReflection()
        {
            var assemblies = NGUI.RuntimeAssemblies;
            if (!excludeAssemblyNames.IsEmpty())
                assemblies = assemblies.Where(_ => _.GetName().ToString().ToLower().Contains(excludeAssemblyNames));

            var types = assemblies.SelectMany(_ => _.GetTypes());
            if (!excludeTypeNames.IsEmpty())
                types = types.Where(type => excludeTypeNames.ToLower().Split(',')
                    .All(typeName => !type.FullName.Contains(typeName)));

            var members = types.SelectMany(type => type.GetMethods(flags))
                .Where(method => method.IsStatic && method.GetParameters().Length == 0);

            return members;
        }

        public void Perform() => action?.Invoke();
    }
}
