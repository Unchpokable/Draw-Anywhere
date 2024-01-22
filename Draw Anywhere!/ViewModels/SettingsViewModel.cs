using System;
using System.Collections.Generic;
using System.Windows.Documents;
using DrawAnywhere.MvvmCore;

namespace DrawAnywhere.ViewModels
{
    internal class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel(MainViewModel host)
        {
            _host = host;
            _childControls = new List<ObservableObject>();

            OpenDirectorySelectionDialog = new RelayCommand(OpenDirectoryDialog);
        }

        public RelayCommand OpenDirectorySelectionDialog { get; set; }

        private readonly MainViewModel _host;

        private List<ObservableObject> _childControls;

        private bool _dialogOpened = false;

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

            }

            CloseAllDialogs();
        }
    }
}
