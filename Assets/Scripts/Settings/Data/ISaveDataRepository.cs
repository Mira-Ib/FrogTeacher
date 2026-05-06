public interface ISaveDataRepository<T>
{
    void Save(T data);
    T Load();
}