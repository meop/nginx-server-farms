using System.Threading.Tasks;

namespace NginxServerFarms.Services {
    internal interface INginxHubClient {
        Task Connect(string hubPath);
    }
}
