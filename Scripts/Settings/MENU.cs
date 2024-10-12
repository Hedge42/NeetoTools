using UnityEngine;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public struct MENU
    {
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
        public const string Neeto = "(ﾉ◕ヮ◕)ﾉ*:･ﾟ✧/";
        public const string Assets = "Assets/" + Neeto;
        public const string Create = "Assets/Create/" + Neeto;
        public const string GameObject = "GameObject/" + Neeto;
        public const string Run = Neeto + "Run/";
        public const string Open = Neeto + "Open/";
        public const string Tools = Neeto + "Tools/";
        public const string Debug = Neeto + "Debug/";
        public const string DebugAsset = Assets + "Debug/";
        public const string Animation = Neeto + "Animation/";
        public const string Dialogue = Neeto + "Dialogue/";

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