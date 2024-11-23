using UnityEngine;

namespace Neeto
{
    public class GetLabelAttribute : PropertyAttribute
    {
        public string property { get; }
        public GetLabelAttribute(string property)
        {
            this.property = property;
            this.order = -1000;
        }
    }
}
