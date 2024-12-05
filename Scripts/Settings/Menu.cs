#if UNITY_EDITOR
#endif

namespace Neeto
{

    public struct Menu
    {
        public static implicit operator string(Menu m) => Main;

        /* (⌐■_■)
        (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧
        ಠ▃ಠ
        (✿◡‿◡)
        ༼ つ ◕_◕ ༽つ
        (。﹏。*)
        ಥ_ಥ
        ಠ_ಠ
        ಠ▃ಠ
        (。_。)
        ( ´_ゝ` )
         */
        public const string Main = "(✿◡‿◡)/";
        public const string Assets = "Assets/" + Main;
        public const string Create = "Assets/Create/" + Main;
        public const string GameObject = "GameObject/" + Main;
        public const string Run = Main + "Run/";
        public const string Open = Main + "Open/";
        public const string Tools = Main + "Tools/";
        public const string Debug = Main + "Debug/";
        public const string DebugAsset = Assets + "Debug/";
        public const string Animation = Main + "Animation/";
        public const string Dialogue = Main + "Dialogue/";

        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Top = -4200;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Higher = -1111;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int High = -420;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Mid = 10;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Low = 420;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Lower = 1111;
        /// <summary>Priority <see cref="UnityEditor.MenuItem.priority"/></summary>
        public const int Bottom = 4200;
    }
}