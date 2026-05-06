public class JsonSettingsRepository : ISaveDataRepository<SettingsData>
{
    private readonly string _filePath;

    public JsonSettingsRepository(string fileName)
    {
        _filePath = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, fileName);
    }

    public void Save(SettingsData data)
    {
        string json = UnityEngine.JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(_filePath, json);
    }

    public SettingsData Load()
    {
        if (System.IO.File.Exists(_filePath))
        {
            string json = System.IO.File.ReadAllText(_filePath);
            return UnityEngine.JsonUtility.FromJson<SettingsData>(json);
        }
        return new SettingsData(); // ファイルがなければデフォルト値を返す
    }
}