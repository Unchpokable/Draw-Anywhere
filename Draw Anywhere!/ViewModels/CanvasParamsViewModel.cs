using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Extensions;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace DrawAnywhere.ViewModels
{
    internal class CanvasParamsViewModel : ObservableObject
    {
        public CanvasParamsViewModel(MainViewModel host)
        {
            _host = host;
        }

        public RelayCommand OpenColorSelection { get; set; }

        public Color CanvasBackgroundColor
        {
            get => _host.BackgroundColor;
            set
            {
                _host.BackgroundColor = value;
                OnPropertyChanged();
            }
        }

        public float CanvasBackgroundOpacity
        {
            get => _host.BackgroundOpacity;
            set
            {
                _host.BackgroundOpacity = value;
                OnPropertyChanged();
            }
        }

        private readonly MainViewModel _host;

        private void OpenColorSelectionDialog(object _)
        {

        }
    }
}
