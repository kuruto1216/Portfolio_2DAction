using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            return null;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSaveData()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSaveData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }
    }
}
