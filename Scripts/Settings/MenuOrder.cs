namespace Neeto
{
    public partial struct MenuOrder
    {
        public static implicit operator int(MenuOrder p) => p.value;
        public static implicit operator MenuOrder(int i) => new MenuOrder { value = i };

        public int value;

        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Min = -4200;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Lower = -1111;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Low = -420;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Mid = 10;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int High = 420;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Higher = 1111;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Max = 4200;
    }
}
