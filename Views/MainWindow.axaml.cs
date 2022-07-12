// System Imports

// Application Imports
using Watcher.Models;
using Watcher.ViewModels;

// Library Imports
using Library.Network.Teller;

using Avalonia.ReactiveUI;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Watcher.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public static MainWindow? Instance { get; private set; }
        
        public static TellerWatcher? Watcher;
        public static WatcherData? Data;

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            Watcher = new();
            Data = new(Watcher);

            var ViewModel = new MainWindowViewModel
            {
                MainWindow = this
            };

            DataContext = ViewModel;

            BindControls();
        }

        void BindControls()
        {
            WatcherDirectoryTextBox = this.FindControl<TextBox>("WatcherDirectory");
            WorkerDirectoryTextBox = this.FindControl<TextBox>("WorkerDirectory");
        }

        void WatcherDirectoryKeyUp(object sender, KeyEventArgs key)
        {
            if (key.Key != Key.Enter)
                return;

            var directory = WatcherDirectoryTextBox.Text;

            Data.ChangeWatcherDirectory(directory);
        }

        void WorkerDirectoryKeyUp(object sender, KeyEventArgs key)
        {
            if (key.Key != Key.Enter)
                return;

            var directory = WorkerDirectoryTextBox.Text;

            if (Data.ChangeWorkerDirectory(directory))
                return;

            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                .GetMessageBoxStandardWindow("Incorrect Directory", $"Given directory {directory} is not valid")
                .Show();
        }

        public void SelectWorkerNode(object sender, RoutedEventArgs args)
        {
            var worker = Watcher?.GetWorkerRPC(Data.SelectedWorker.Data);

            if (worker == null)
                return;
            
            var target = System.IO.Path.GetFullPath(worker?.WorkingDirectory + "\\" + Data?.SelectedWorkerNode);

            Data?.ChangeWorkerDirectory(target);
        }

        public void SelectWatcherNode(object sender, RoutedEventArgs args)
        {
            var target = System.IO.Path.GetFullPath(Watcher?.WorkingDirectory + "\\" + Data?.SelectedWatcherNode);

            Data?.ChangeWatcherDirectory(target);
        }

        public TextBox WatcherDirectoryTextBox { get; private set; }

        public TextBox WorkerDirectoryTextBox { get; private set; }
    }
}
