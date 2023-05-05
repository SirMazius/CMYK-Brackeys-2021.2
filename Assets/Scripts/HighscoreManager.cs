using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class HighscoreManager : SingletonMono<HighscoreManager>
{
    public class ScoreRecord
    {
        public string Name = "Local";
        public int Score = 0;

        public ScoreRecord(int score)
        {
            Score = score;
        }
    }

    public List<ScoreRecord> ScoreRecords = new List<ScoreRecord>();
    public const int MaxScoreRecords = 10;
    public const string RecordsStorageFile = "Highscores";
    

    public void Start()
    {
        ReadRecordsFromFile();
    }

    //Leemos el fichero local y cargamos la lista de records
    [Button, SerializeField, ExecuteAlways]
    private void ReadRecordsFromFile()
    {
        string jsonText;
        string path = GetJsonLocalPath(RecordsStorageFile);
        StreamReader reader = new StreamReader(path);
        jsonText = reader.ReadToEnd();
        ScoreRecords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ScoreRecord>>(jsonText);
        reader.Close();
    }

    private string GetJsonLocalPath(string jsonsFolderPath)
    {
        return Path.Combine(jsonsFolderPath, RecordsStorageFile + ".json");
    }

    //Gruardamos el json de la lista de records en un fichero local
    [Button, SerializeField, ExecuteAlways]
    private void SaveRecordsOnFile()
    {
        string jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(ScoreRecords);
        string path = GetJsonLocalPath(RecordsStorageFile);
        if (!Directory.Exists(GameGlobals.RecordsStoragePath))
            Directory.CreateDirectory(GameGlobals.RecordsStoragePath);
        FileStream file = new FileStream(path, FileMode.Create);
        file.Write(Encoding.UTF8.GetBytes(jsonText), 0, Encoding.UTF8.GetByteCount(jsonText));
        file.Close();
    }

    //Añadimos un record a la lista en el orden correcto y truncando si hay mas records de los permitidos
    public void TryAddNewRecord(ScoreRecord record)
    {
        for (int i = 0; i < MaxScoreRecords; i++)
        {
            if (i >= ScoreRecords.Count || ScoreRecords[i].Score < record.Score)
            {
                ScoreRecords.Insert(i, record);
                TrunkateRecordsList();
                break;
            }
        }

        SaveRecordsOnFile();
    }

    [Button, ExecuteAlways]
    public void TryAddNewRecord(int score)
    {
        TryAddNewRecord(new ScoreRecord(score));
    }

    //Eliminamos los resgistros excedidos de la lista de records
    public void TrunkateRecordsList()
    {
        while(ScoreRecords.Count > MaxScoreRecords)
        {
            ScoreRecords.RemoveAt(ScoreRecords.Count - 1);
        }
    }
}
