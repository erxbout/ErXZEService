namespace ErXZEService.DependencyServices
{
    public interface IStoragePathService
    {
        string ExternalStorage { get; }

        string AppStorage { get; }
    }
}
