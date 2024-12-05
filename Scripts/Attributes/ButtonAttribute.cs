
namespace Neeto
{
    public class ButtonAttribute : System.Attribute
    {
        public string label { get; private set; }
        public ButtonAttribute(string label = null)
        {
            this.label = label;
        }
    }
}