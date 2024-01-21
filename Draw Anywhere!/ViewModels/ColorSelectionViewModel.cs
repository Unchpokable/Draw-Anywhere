using System.Windows.Media;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Sys;

namespace DrawAnywhere.ViewModels
{
    internal class ColorSelectionViewModel : ObservableObject
    {
        public ColorSelectionViewModel(ByRef<Color> baseColor)
        {
            _selectedColor = baseColor;
        }

        public Color SelectedColor
        {
            get => _selectedColor.Value;
            set
            {
                _selectedColor.Value = value;
                OnPropertyChanged();
            }
        }

        private ByRef<Color> _selectedColor;
    }
}
