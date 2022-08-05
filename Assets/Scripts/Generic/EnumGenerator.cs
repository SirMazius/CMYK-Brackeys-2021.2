#if UNITY_EDITOR

using UnityEditor;
using System.IO;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class EnumGenerator : SingletonMono<EnumGenerator>
{
    [Button]
    public static void GenerateEnum(string enumName, List<string> enumValues)
    {
        string filePathAndName = "Assets/Scripts/Enums/" + enumName + ".cs"; //The folder Scripts/Enums/ is expected to exist

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
        {
            streamWriter.WriteLine("public enum " + enumName);
            streamWriter.WriteLine("{");
            for (int i = 0; i < enumValues.Count; i++)
            {
                streamWriter.WriteLine("\t" + enumValues[i].ToEnumFormat() + ",");
            }
            streamWriter.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }
}

#endif