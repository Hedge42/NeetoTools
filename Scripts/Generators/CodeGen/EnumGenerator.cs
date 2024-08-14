using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using System;
using System.IO;
using System.Linq;
using Neeto;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Neeto
{
    public class EnumGenerator
    {
        public static void GenerateEnums(Type type, string path)
        {
            // Use reflection to find all subclasses of the base class
            var subclasses = type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t));

            // Start building the string for the Enum
            string enumString = "public enum Subclasses\n{\n";
            foreach (var subclass in subclasses)
            {
                enumString += "\t" + subclass.Name + ",\n";
            }
            enumString += "}";

            // Write the Enum to a file
            File.WriteAllText(Application.dataPath + path, enumString);

            // Refresh the AssetDatabase to ensure Unity recognizes the new file
            AssetDatabase.Refresh();
        }
    }
}