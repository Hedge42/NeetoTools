namespace Neeto
{
    public partial struct Priority
    {
        public static implicit operator int(Priority p) => p.value;
        public static implicit operator Priority(int i) => new Priority { value = i };

        public int value;
    }
}
