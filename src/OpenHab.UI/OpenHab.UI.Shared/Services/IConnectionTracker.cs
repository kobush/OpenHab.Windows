using System.Threading.Tasks;

namespace OpenHab.UI.Services
{
    public interface IConnectionTracker
    {
        Task<bool> CheckConnectionAsync();
        bool IsConnected { get; }
    }
}