// System Usings
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// Application Usings
using Watcher.Views;
using Watcher.Models;

// External Usings
using Avalonia.Controls;
using ReactiveUI;

using CommunityToolkit.Mvvm.Input;

using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using MessageBox.Avalonia;


namespace Watcher.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindow? MainWindow;
        public static WatcherData? Data => MainWindow.Data;

        public string ApplicationName => $"Remote Teller{WorkerInformation}";
        public string Title => "Remote Teller";
        public string WorkerInformation
        {
            get
            {
                if (!MainWindow.Data.Broker.Available) return " - Not ready (Broker not available)";
                if (MainWindow.Data?.ActiveWorker == null) return " - Ready";

                return $" - Watching {MainWindow.Data.ActiveWorker.Name} * {MainWindow.Data.ActiveWorker.Address} @" +
                    $" {MainWindow.Data.ActiveWorker.UUID}";
            }
        }

        public string ConnectionState
        {
            get
            {
                if (!MainWindow.Data.Broker.Available)
                    return $"Broker API is not available at {MainWindow.Data.Broker.Address}";

                if (MainWindow.Data?.ActiveWorker == null)
                    return "Ready to connect";

                return $"Connected to {MainWindow.Data.ActiveWorker.Address}";
            }
        }

        public ObservableCollection<string> WorkerIds => MainWindow.Data?.WorkerIds;

        private string _workerId;
        public string WorkerId
        {
            get => _workerId;
            set
            {
                MainWindow.Data?.SelectWorker(value);
                MainWindow.Data?.RaiseAndSetIfChanged(ref _workerId, value);
            }
        }

        public MainWindowViewModel()
        {
            if (MainWindow == null)
                return;

            Data?.ChangeWatcherDirectory(MainWindow.Watcher.WorkingDirectory);
        }

        //

        [ICommand]
        public async void Connect()
        {
            if (MainWindow.Data?.SelectedWorker == null)
                return;

            var targetInformation = $"{MainWindow.Data?.SelectedWorker?.UUID} @ {MainWindow.Data?.SelectedWorker?.Address}";

            var message = MessageBoxManager
                .GetMessageBoxCustomWindow(new MessageBoxCustomParams
                {
                    ContentTitle = "Connecting to Worker",
                    ContentMessage = $"Connecting to {targetInformation}\n",

                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,

                    Icon = Icon.Wifi,
                    SystemDecorations = SystemDecorations.Full,


                    ButtonDefinitions = new []
                    {
                        new ButtonDefinition() {
                            Name = "Cancel",
                            IsCancel = true
                        }
                    }
                });
        
            var connected = await MainWindow.Data?.Connect();
            // var er = await message.Show();


            this.RaisePropertyChanged(nameof(WorkerInformation));
            this.RaisePropertyChanged(nameof(ConnectionState));
        }

        //

        [ICommand]
        public async void WorkerDirectoryBack()
        {
            if (Data?.WatcherDirectory == null)
                return;

            var directory = System.IO.Directory.GetParent(Data.WorkerDirectory);

            if (directory == null)
                return;

            Data.ChangeWorkerDirectory(directory.FullName);
        }

        [ICommand]
        public async void WatcherDirectoryBack()
        {
            if (Data?.WatcherDirectory == null)
                return;

            var directory = System.IO.Directory.GetParent(Data.WatcherDirectory);

            if (directory == null)
                return;

            Data.ChangeWatcherDirectory(directory.FullName);
        }

        [ICommand]
        public async Task TransferToWorker()
        {
            Data.TransferToWorker();
        }

        [ICommand]
        public async Task TransferToWatcher()
        {
            Data?.TransferToWatcher();
        }

    }
}
