
namespace PartCover.Browser.Api
{
    public interface IServiceContainer
    {
        T GetService<T>() where T : class;

        bool RegisterService<T>(T service) where T : class;

        bool UnregisterService<T>(T service) where T : class;
    }
}
