using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif


public interface IGameMode
{
    public static IGameMode current { get; private set; }

    public static Action<IGameMode, IGameMode> onChanged;
}