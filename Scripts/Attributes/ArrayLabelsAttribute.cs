using System;
using UnityEngine;

namespace Matchwork
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class ArrayLabelsAttribute : Attribute
    {
        public string propertyName;
        public ArrayLabelsAttribute(string propertyName)
        {
            this.propertyName = propertyName;
        }
    }
}