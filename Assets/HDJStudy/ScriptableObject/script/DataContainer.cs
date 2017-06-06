using UnityEngine;

using UnityEditor;

using System.Collections.Generic;

using System.IO;
using Kent.Boogaart.KBCsv;

public class DataContainer  {

    public TextAsset text;

    [MenuItem("Assets/Export")]
    public static void Excute()
    {
        DataParse data = ScriptableObject.CreateInstance<DataParse>();
        data.theData = ParserShieldStrings(File.ReadAllBytes(Application.dataPath + "/HDJStudy/ScriptableObject/Text/shield.txt"));
        string path = "Assets/HDJStudy/ScriptableObject/Text/shieldt.asset";
        AssetDatabase.CreateAsset(data, path);
        Object o = AssetDatabase.LoadAssetAtPath(path, typeof(DataParse));

        //打包为SysData.assetbundle文件。                 

        BuildPipeline.BuildAssetBundle(o, null, Application.dataPath + "/HDJStudy/ScriptableObject/Text/SysData.assetbundle");

        //删除面板上的那个临时对象               

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.Refresh();
    }

    public static string[] ParserShieldStrings(byte[] data)
    {
        var reader = new StreamReader(new MemoryStream(data));
        using (var csReader = new CsvReader(reader))
        {
            List<string> listWord = new List<string>();
            foreach (var str in csReader.ShiedRecord())
            {
                if (!listWord.Contains(str))
                    listWord.Add(str);
            }
            return listWord.ToArray();
        }
    }

}
