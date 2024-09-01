using System.ComponentModel;
using System.Windows.Media;
using DrawAnywhere.Models;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Sys;
using DrawAnywhere.ViewModels.Helpers;

namespace DrawAnywhere.ViewModels
{
    internal class CanvasParamsViewModel : ObservableObject
    {
        public CanvasParamsViewModel(MainViewModel host, OpacityColor canvasColor, AppConfig config)
        {
            _config = config;
            _host = host;
            _canvasColor = canvasColor;
            _initialParams = canvasColor;
            _canvasColorBase = new ByRef<Color>(_canvasColor.BaseColor);
            OpenColorSelection = new RelayCommand(OpenColorSelectionDialog);
            ApplyAndClose = new RelayCommand(ApplyAndCloseInternal);
            RevertAndClose = new RelayCommand(RevertAndCloseInternal);
        }

        public RelayCommand OpenColorSelection { get; set; }

        public RelayCommand ApplyAndClose { get; set; }
        public RelayCommand RevertAndClose { get; set; }

        public Color CanvasBackgroundColor
        {
            get => _canvasColor.BaseColor;
            set
            {
                _canvasColor.BaseColor = value;
                _host.Background = _canvasColor;
                OnPropertyChanged();
            }
        }

        public float CanvasBackgroundOpacity
        {
            get => _canvasColor.Opacity;
            set
            {
                _canvasColor.Opacity = value;
                _host.Background = _canvasColor;
                OnPropertyChanged();
            }
        }

        private AppConfig _config;
        private OpacityColor _initialParams;
        private OpacityColor _canvasColor;
        private ByRef<Color> _canvasColorBase;
        private ColorSelectionViewModel _colorSelectionViewModel;
        private readonly MainViewModel _host;

        private bool _dialogOpened;

        public void CloseDialog()
        {
            if (_colorSelectionViewModel == null)
                return;

            _colorSelectionViewModel.PropertyChanged -= OnColorSelectionPropertyChanged;
            _host.CloseChild(_colorSelectionViewModel); 

            _dialogOpened = false;
        }

        private void OpenColorSelectionDialog(object _)
        {
            if (_dialogOpened)
                return;

            if (_colorSelectionViewModel != null)
            {
                CloseDialog();
            }
            
            _colorSelectionViewModel = new ColorSelectionViewModel(_canvasColorBase);
            _colorSelectionViewModel.PropertyChanged += OnColorSelectionPropertyChanged;
            _host.AppendControlChild(_colorSelectionViewModel);
            _dialogOpened = true;
        }

        private void OnColorSelectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(_colorSelectionViewModel.SelectedColor))
                return;

            CanvasBackgroundColor = _canvasColorBase.Value;
        }

        private void ApplyAndCloseInternal(object _)
        {
            CloseDialog();
            _config.CanvasBackgroundColor = CanvasBackgroundColor;
            _config.CanvasBackgroundOpacity = CanvasBackgroundOpacity;
            _config.Save();
            _host.CloseChild(this);
            Notifications.ShowSuccess("Done!", "Parameters saved!");
        }

        private void RevertAndCloseInternal(object _)
        {
            _host.Background = _initialParams;
            ApplyAndCloseInternal(_);
        }
    }
}
