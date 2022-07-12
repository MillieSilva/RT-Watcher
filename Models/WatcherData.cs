// System Usings
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

// Application Usings

// External Usings
using Library.Network.Broker;
using Library.Network.Teller;

using ReactiveUI;


namespace Watcher.Models
{
    public class WatcherData : ReactiveObject
    {
        public static TellerWatcher? Watcher;

        public BrokerWatcherRestClient Broker => Watcher.BrokerClient;

        public WatcherData(TellerWatcher watcher)
        {
            Watcher = watcher;

            RefreshWorkerIds();
            RefreshWorkersMetadata();
        }

        private void RefreshWorkersMetadata()
        {
            WorkersMetadata.Clear();
            
            foreach (var workerMetadata in Watcher.WorkersMetadata)
                WorkersMetadata.Add(new WorkerMetadataModel(workerMetadata, Watcher));
        }

        private void RefreshWorkerIds()
        {
            WorkerIds.Clear();

            foreach (var workerId in Watcher.WorkerIds)
                WorkerIds.Add(workerId.ToString());
        }

        //

        public ObservableCollection<string> WorkerIds { get; private set; } = new();
        public ObservableCollection<WorkerMetadataModel> WorkersMetadata { get; set; } = new();

        private WorkerMetadataModel activeWorker;
        public WorkerMetadataModel ActiveWorker
        {
            get => activeWorker;
            set
            {
                this.RaiseAndSetIfChanged(ref activeWorker, value);
                this.RaisePropertyChanged(nameof(ActiveWorker));
                this.RaisePropertyChanged(nameof(HasActiveWorker));
            }
        }

        public bool HasActiveWorker => ActiveWorker != null;
        // public bool HasActiveWorker => true;

        internal void SelectWorker(string value)
        {
            var uuid = Guid.Parse(value);

            var workerMetadata = Watcher.FetchWorkerMetadata(uuid);

            if (workerMetadata == null)
                return;

            var workerMetadataModel = new WorkerMetadataModel((WorkerMetadata)workerMetadata, Watcher);
            
            WorkersMetadata.Add(workerMetadataModel);
            
            RefreshWorkersMetadata();

            SelectedWorker = workerMetadataModel;
        }

        //

        public bool HasWorker => SelectedWorker != null;
        public bool CanConnect => SelectedWorker != null && SelectedWorker.CanConnect;

        private WorkerMetadataModel selectedWorker;
        public WorkerMetadataModel SelectedWorker
        {
            get => selectedWorker;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedWorker, value);

                this.RaisePropertyChanged(nameof(HasWorker));
                this.RaisePropertyChanged(nameof(CanConnect));
            }
        }

        internal async Task<bool> Connect()
        {
            if (Watcher == null)
                return false;

            Watcher?.Connect(SelectedWorker.Data);

            var connected = await Watcher.GetWorkerRPC(SelectedWorker.Data).Connect();

            if (connected)
                ActiveWorker = SelectedWorker;

            ChangeWorkerDirectory(ActiveWorker.WorkerDirectory);

            return connected;
        }

        public string WorkerDirectory => ActiveWorker?.WorkerDirectory;
        public ObservableCollection<string> WorkerDirectoryTree => ActiveWorker?.WorkerDirectoryTree;

        public string selectedWorkerNode;
        public string SelectedWorkerNode
        {
            get => selectedWorkerNode;
            set => selectedWorkerNode = value;
        }

        public string SourceWorkerPath
        {
            get
            {
                var worker = Watcher?.GetWorkerRPC(SelectedWorker.Data);

                if (worker == null)
                    return "";

                return System.IO.Path.GetFullPath(worker?.WorkingDirectory + "\\" + SelectedWorkerNode); ;
            }
        }
        public string TargetWorkerPath
        {
            get
            {
                var worker = Watcher?.GetWorkerRPC(SelectedWorker.Data);

                if (worker == null)
                    return "";

                return System.IO.Path.GetFullPath(worker?.WorkingDirectory + "\\" + SelectedWatcherNode); ;
            }
        }

        //

        internal bool ChangeWorkerDirectory(string directory)
        {
            var worker = Watcher?.GetWorkerRPC(SelectedWorker.Data);

            if (worker == null)
                return false;

            Task.Run(() =>
            {
                worker.ChangeDirectory(directory).GetAwaiter().GetResult();
                ChangeWatcherDirectory(Watcher?.WorkingDirectory);

                this.RaisePropertyChanged(nameof(WorkerDirectory));
                this.RaisePropertyChanged(nameof(WorkerDirectoryTree));
            });

            return true;
        }

        internal void RefreshWorkerDirectory()
        {
            var worker = Watcher?.GetWorkerRPC(SelectedWorker.Data);

            if (worker == null)
                return;

            ChangeWorkerDirectory(worker.WorkingDirectory);
        }

        //

        public string WatcherDirectory => Watcher?.WorkingDirectory;
        public ObservableCollection<string> WatcherDirectoryTree => Watcher?.DirectoryTree;

        public string selectedWatcherNode;
        public string SelectedWatcherNode
        {
            get => selectedWatcherNode;
            set => selectedWatcherNode = value;
        }

        public string SourceWatcherPath => System.IO.Path.GetFullPath(Watcher?.WorkingDirectory + "\\" + SelectedWatcherNode);
        public string TargetWatcherPath => System.IO.Path.GetFullPath(Watcher?.WorkingDirectory + "\\" + SelectedWorkerNode);

        //

        internal void ChangeWatcherDirectory(string directory)
        {
            Watcher?.ChangeDirectory(directory);

            this.RaisePropertyChanged(nameof(WatcherDirectory));
            this.RaisePropertyChanged(nameof(WatcherDirectoryTree));
        }

        internal void RefreshWatcherDirectory()
        {
            ChangeWatcherDirectory(WatcherDirectory);
        }

        //

        bool isTransfering;
        public bool IsTransfering
        {
            get => isTransfering;
            set 
            {
                this.RaiseAndSetIfChanged(ref isTransfering, value);
            }
        }

        internal void TransferToWorker()
        {
            var worker = Watcher?.GetWorkerRPC(SelectedWorker.Data);

            if (worker == null)
                return;
            
            worker?.TransferFile(SourceWatcherPath, TargetWorkerPath);
            RefreshWorkerDirectory();
        }

        internal void TransferToWatcher()
        {
            var worker = Watcher?.GetWorkerRPC(SelectedWorker.Data);

            if (worker == null)
                return;

            IsTransfering = true;
            worker?.RequestFile(SourceWorkerPath, TargetWatcherPath);

            RefreshWatcherDirectory();
            IsTransfering = false;
        }
    }
}
