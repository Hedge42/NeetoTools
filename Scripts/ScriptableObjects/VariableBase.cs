using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableBase : ScriptableObject { }
public class Variable<T> : VariableBase
{
    public static implicit operator T(Variable<T> variable) => variable.value;

    [SerializeField]
    protected T value;

    public virtual T Value
    {
        get => value;
        set => this.value = value;
    }
}
