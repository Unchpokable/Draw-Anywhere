using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using DrawAnywhere.Models;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Sys;

namespace DrawAnywhere.ViewModels
{
    internal class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel(MainViewModel host)
        {
            _config = AppConfig.Instance(); // This instance should be loaded by MainViewModel

            _host = host;
            _childControls = new List<ObservableObject>();

            AutoRunEnabled = _config.WindowsStartupEnabled;

            ApplyAndClose = new RelayCommand(ApplyChanges);
            OpenDirectorySelectionDialog = new RelayCommand(OpenDirectoryDialog);
        }

        public string ScreenShotsDirectory
        {
            get => _config.ScreenShotPath;
            set
            {
                _config.ScreenShotPath = value;
                OnPropertyChanged();
            }
        }

        public bool AutoRunEnabled
        {
            get => _config.WindowsStartupEnabled;
            set
            {
                _config.WindowsStartupEnabled = value; 
                OnPropertyChanged();
                ToggleAutoRun();
            }
        }

        public bool CleanCanvasOnHide
        {
            get => _config.CleanCanvasWhenHide;
            set
            {
                _config.CleanCanvasWhenHide = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand ApplyAndClose { get; set; }
        public RelayCommand OpenDirectorySelectionDialog { get; set; }

        private readonly MainViewModel _host;

        private List<ObservableObject> _childControls;

        private bool _dialogOpened = false;

        private AppConfig _config;

        public void CloseAllDialogs()
        {
            _dialogOpened = false;

            foreach (var ctrl in _childControls)
                _host.CloseChild(ctrl);
        }

        private void OpenDirectoryDialog(object _)
        {
            if (_dialogOpened)
                return;

            var vm = new DirectoryBrowserViewModel(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            vm.Completed += OnDialogClosed;

            _host.AppendControlChild(vm);
            _childControls.Add(vm);

            _dialogOpened = true;
        }

        private void OnDialogClosed(object sender, DirectoryPresentViewModel e)
        {
            _dialogOpened = false;
            if (e != null)
            {
                ScreenShotsDirectory = e.FullPath;
            }

            CloseAllDialogs();

            var typedSender = (DirectoryBrowserViewModel)sender;
            typedSender.Completed -= OnDialogClosed;
        }

        private void ToggleAutoRun()
        {
            if (_config.WindowsStartupEnabled)
                WindowsShell.AddStartup();
            else 
                WindowsShell.RemoveStartup();
        }

        private async void ApplyChanges(object _)
        { 
            await _config.SaveAsync();
            Notifications.ShowSuccess("Done!", "Settings saved!");
        }
    }
}
