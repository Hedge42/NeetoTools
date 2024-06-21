//using Cysharp.Threading.Tasks;
using UnityEngine;

namespace mtk
{
    public class DropdownAttribute : PropertyAttribute
    {
        public string factory { get; private set; }
        public DropdownAttribute(string factory) => this.factory = factory;
    }
}
