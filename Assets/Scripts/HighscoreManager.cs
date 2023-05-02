using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class HighscoreManager : SerializedMonoBehaviour
{
    public class HighscoreRecord
    {
        public string Name = "Local";
        public int Score = 0;

        public HighscoreRecord(int score)
        {
            Score = score;
        }
    }

    public const int MaxRecords = 10;
    public const string RecordsStoragePath = "Highscores";
    public const string RecordsStorageFile = "Highscores";

    public List<HighscoreRecord> Records = new List<HighscoreRecord>();


    public void Start()
    {
        //ReadRecordsFromFile();
    }

    public static string GetJsonLocalPath(string fileName)
    {
        return Path.Combine(RecordsStoragePath, fileName + ".json");
    }

    //Leemos el fichero local y cargamos la lista de records
    [Button, SerializeField, ExecuteAlways]
    private void ReadRecordsFromFile()
    {
        string jsonText;
        string path = GetJsonLocalPath(RecordsStorageFile);
        StreamReader reader = new StreamReader(path);
        jsonText = reader.ReadToEnd();
        Records = Newtonsoft.Json.JsonConvert.DeserializeObject<List<HighscoreRecord>>(jsonText);
    }

    //Gruardamos el json de la lista de records en un fichero local
    [Button, SerializeField, ExecuteAlways]
    private void SaveRecordsOnFile()
    {
        string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(Records);
        string path = GetJsonLocalPath(RecordsStorageFile);
        if (!Directory.Exists(RecordsStoragePath))
            Directory.CreateDirectory(RecordsStoragePath);
        FileStream file = new FileStream(path, FileMode.Create);
        file.Write(Encoding.UTF8.GetBytes(jsonText), 0, Encoding.UTF8.GetByteCount(jsonText));
        file.Close();
    }

    //Añadimos un record a la lista en el orden correcto y truncando si hay mas records de los permitidos
    public void AddNewRecord(HighscoreRecord record)
    {
        for (int i = 0; i < Records.Count; i++)
        {
            if (Records[i].Score < record.Score)
            {
                Records.Insert(i, record);
                TrunkateRecordsList();
                break;
            }
        }

        SaveRecordsOnFile();
    }

    [Button]
    public void AddNewRecord(int score)
    {
        AddNewRecord(new HighscoreRecord(score));
    }

    //Eliminamos los resgistros excedidos de la lista de records
    public void TrunkateRecordsList()
    {
        while(Records.Count > MaxRecords)
        {
            Records.RemoveAt(Records.Count - 1);
        }
    }
}
