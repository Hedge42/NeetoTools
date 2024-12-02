using System;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Neeto.Exp
{
    [Serializable]
    public class SerializedArgument
    {
        public UnityEngine.Object UnityObject;
        [SerializeReference] public object ReferenceObject;
        public Value ValueObject;

        public ArgumentType ArgumentType;
        public string ObjectType;
        public Type Type => Type.GetType(ObjectType);
        public object Value => ArgumentType switch
        {
            ArgumentType.UnityObject => UnityObject,
            ArgumentType.ReferenceObject => ReferenceObject,
            ArgumentType.ValueObject => ValueObject,
            _ => null
        };
        public static ArgumentType GetArgumentType(Type type)
        {
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                return ArgumentType.UnityObject;
            else
                return ArgumentType.ValueObject;
            // TODO
        }
    }
    public enum ArgumentType
    {
        NotSupported,
        UnityObject,
        ReferenceObject,
        ValueObject
    }
}
