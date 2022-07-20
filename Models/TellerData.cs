// System Imports
using System.Collections.ObjectModel;
using System.Linq;

// Application Imports

// External Imports
using Library.Network.Teller;

using ReactiveUI;


namespace Watcher.Models
{
    public class WorkerMetadataModel : ReactiveObject
    {
        internal TellerWatcher Watcher { get; init; }
        internal WorkerMetadata Data { get; init; }

        public WorkerMetadataModel(WorkerMetadata workerMetadata, TellerWatcher watcher)
        {
            Watcher = watcher;
            Data = workerMetadata;
        }

        public string? UUID => Data.UUID.ToString();

        public string? Name = "";
        public string? Address => Data.Public_IPV4.Last();

        //

        public string? DisplayName => Name;
        public string? DisplayUUID => $"Id: {UUID}";
        public string? DisplayAddress => $"IP: {Address}";

        public string? DisplayStatus {
            get
            {
                if (!Watcher.WorkersConnections.ContainsKey(Data.UUID))
                    return null;

                return Watcher.WorkersConnections[Data.UUID].State.ToString();
            }
        }

        public bool CanConnect {
            get
            {
                if (!Watcher.WorkersConnections.ContainsKey(Data.UUID))
                    return false;

                return Watcher.WorkersConnections[Data.UUID].State == ConnectionState.Available;
            }
        }

        public string WorkerDirectory => Watcher?.GetWorkerRPC(Data)?.WorkingDirectory;
        public ObservableCollection<string> WorkerDirectoryTree 
        {
            get
            {
                return Watcher?.GetWorkerRPC(Data).DirectoryTree;
            }
        }
    }
}
