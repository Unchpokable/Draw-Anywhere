using System;
using System.IO;
using DrawAnywhere.MvvmCore;

namespace DrawAnywhere.ViewModels
{
    internal class DirectoryPresentViewModel : ObservableObject
    {
        public DirectoryPresentViewModel(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("Given directory does not exists");

            _info = new DirectoryInfo(path);
        }

        public DirectoryPresentViewModel(DirectoryInfo directory)
        {
            _info = directory;
        }

        public string Name => _info.Name;

        public string FullPath => _info.FullName;

        public string Parent => _info.Parent?.FullName;

        private DirectoryInfo _info;
    }
}
