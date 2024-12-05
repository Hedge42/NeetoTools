using System;

namespace Neeto
{
    public class QuickMenuAttribute : System.Attribute
    {
        public Func<string> getLabel;
        public int order;

        public QuickMenuAttribute(int order = 0)
        {
            this.order = order;
        }
        public QuickMenuAttribute(string label, int order = 0)
        {
            getLabel = () => label;
            this.order = order;
        }
        public QuickMenuAttribute(Func<string> getLabel, int order = 0)
        {
            this.getLabel = getLabel;
            this.order = order;
        }
    }
}