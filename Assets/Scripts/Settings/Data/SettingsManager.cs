public class SettingsManager
{
    private readonly ISaveDataRepository<SettingsData> _repository;
    public SettingsData CurrentData { get; private set; }

    // インターフェースを注入する（Dependency Injection）
    public SettingsManager(ISaveDataRepository<SettingsData> repository)
    {
        _repository = repository;
        CurrentData = _repository.Load();
    }

    public void UpdateBgmVolume(int volume)
    {
        CurrentData.BgmVolume = volume;
        _repository.Save(CurrentData);
    }

    public void UpdateSeVolume(int volume)
    {
        CurrentData.SeVolume = volume;
        _repository.Save(CurrentData);
    }

}