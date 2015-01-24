using System;
using System.Threading.Tasks;
using OpenHab.Client;

namespace OpenHab.UI.Services
{
    public interface IConnectionTracker
    {
        Task<bool> CheckConnectionAsync();
        bool IsConnected { get; }

        Task Execute(Func<OpenHabRestClient, Task> action);
        Task<T> Execute<T>(Func<OpenHabRestClient, Task<T>> action);
    }
}