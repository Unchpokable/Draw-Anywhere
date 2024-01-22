using DrawAnywhere.MvvmCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace DrawAnywhere.ViewModels
{

    internal class DirectoryBrowserViewModel : ObservableObject
    {
        public DirectoryBrowserViewModel(string root = @"C:\") 
        {
            CurrentDirectories = new ObservableCollection<DirectoryPresentViewModel>();

            CurrentDirectories.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CurrentDirectories));

            SelectDirectory = new RelayCommand(SetDirectorySelection);
            WalkDirectory = new RelayCommand(EnumerateDirectory);
            WalkPreviousDirectory = new RelayCommand(WalkPreviousDir);

            Accept = new RelayCommand(AcceptDialog);
            Close = new RelayCommand(CloseDialog);

            EnumerateDirectory(root);
            SelectedDirectory = new DirectoryPresentViewModel(root);
        }

        public event EventHandler<DirectoryPresentViewModel> Completed;

        public string CurrentRoot
        {
            get => _currentRoot;
            set
            {
                _currentRoot = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DirectoryPresentViewModel> CurrentDirectories { get; set; }

        public DirectoryPresentViewModel SelectedDirectory
        {
            get => _selectedDirectory;
            set
            {
                _selectedDirectory = value;
                OnPropertyChanged();
            }
        }

        public string BindingUserProfilePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public string BindingMyPicturesPath => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public string BindingMyDocumentsPath => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string BindingMyComputerPath => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        public RelayCommand SelectDirectory { get; set; }
        public RelayCommand WalkDirectory { get; set; }
        public RelayCommand WalkPreviousDirectory { get; set; }
        public RelayCommand Accept { get; set; }
        public RelayCommand Close { get; set; }

        private DirectoryPresentViewModel _selectedDirectory;
        private string _currentRoot;

        public void SetItemSelected(int index)
        {
            if (index < 0 || index >= CurrentDirectories.Count)
                return;

            SelectedDirectory = CurrentDirectories[index];
        }

        private void SetDirectorySelection(object directory)
        {
            if (directory is not DirectoryPresentViewModel directoryModel)
                return;

            SelectedDirectory = directoryModel;
        }

        private void EnumerateDirectory(object directory)
        {
            List<DirectoryPresentViewModel> directories;

            if (directory is DirectoryPresentViewModel directoryModel)
            {
                directories = Directory.EnumerateDirectories(directoryModel.FullPath)
                    .Select(dir => new DirectoryPresentViewModel(dir)).ToList();
                CurrentRoot = directoryModel.FullPath;
            }

            else if (directory is string directoryPath)
            {
                directories = Directory.EnumerateDirectories(directoryPath)
                    .Select(dir => new DirectoryPresentViewModel(dir)).ToList();
                CurrentRoot = directoryPath;
            }
            else
                return;

            SetDirectoriesView(directories);
        }

        private void WalkPreviousDir(object _)
        {
            if (_selectedDirectory == null)
                return;

            var parentDir = _selectedDirectory.Parent;

            if (parentDir == null)
                return;

            var directories = Directory.EnumerateDirectories(parentDir)
                .Select(dir => new DirectoryPresentViewModel(dir));

            SetDirectoriesView(directories);
            CurrentRoot = _selectedDirectory.Parent;
            SelectedDirectory = new DirectoryPresentViewModel(_selectedDirectory.Parent);
        }

        private void SetDirectoriesView(IEnumerable<DirectoryPresentViewModel> list)
        {
            CurrentDirectories.Clear();

            foreach (var dir in list)
                CurrentDirectories.Add(dir);
        }

        private void AcceptDialog(object _)
        {
            Completed?.Invoke(this, SelectedDirectory);
        }

        private void CloseDialog(object _)
        {
            Completed?.Invoke(this, null);
        }
    }
}
