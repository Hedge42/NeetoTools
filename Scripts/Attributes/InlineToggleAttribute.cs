using UnityEngine;

namespace Neeto
{
    public class InlineToggleAttribute : PropertyAttribute
    {
        public string fieldName { get; }
        public bool dimWhenDisabled { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName">name of the boolean field that the toggle represents</param>
        /// <param name="isSibling">false: the property is a child</param>
        public InlineToggleAttribute(string fieldName, bool dim = false)
        {
            this.fieldName = fieldName;
            this.dimWhenDisabled = dim;
        }
    }
}
