using System;
using System.Collections.ObjectModel;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Sys;
using GlobalHotKey;

namespace DrawAnywhere.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            VisibleControls = new ObservableCollection<ObservableObject>();
            VisibleControls.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(VisibleControls)); };
            _penColor = new ByRef<Color>(Colors.AliceBlue);
            
            DrawingAttributes = new DrawingAttributes()
                { Color = _penColor.Value, FitToCurve = true, Height = 15, Width = 15 };

            _colorSelection = new ColorSelectionViewModel(_penColor);
            _penConfig = new PenConfigViewModel(DrawingAttributes);
            _penConfig.PropertyChanged += (s, e) => OnPropertyChanged(nameof(DrawingAttributes));
            
            ShowTool = new RelayCommand(ShowUserTool);

            _penColor.ValueChanged += UpdateDrawingAttributes;
            
            RegisterKeyBindings();
        }

        public RelayCommand ShowTool { get; set; }

        public ObservableCollection<ObservableObject> VisibleControls { get; set; }

        public DrawingAttributes DrawingAttributes
        {
            get => _drawingAttributes;
            set
            {
                _drawingAttributes = value;
                OnPropertyChanged();
            }
        }

        private DrawingAttributes _drawingAttributes;

        private ColorSelectionViewModel _colorSelection;
        private PenConfigViewModel _penConfig;

        private ByRef<Color> _penColor;

        private void ShowUserTool(object parameter)
        {
            if (parameter is not ModalWindowType toolType)
                return;

            switch (toolType)
            {
                case ModalWindowType.ColorPicker:
                    if (VisibleControls.Contains(_colorSelection))
                    {
                        VisibleControls.Remove(_colorSelection);
                        return;
                    }

                    if (VisibleControls.Contains(_penConfig))
                    {
                        VisibleControls.Remove(_penConfig);
                    }

                    VisibleControls.Add(_colorSelection);
                    break;

                case ModalWindowType.PenConfig:
                    if (VisibleControls.Contains(_penConfig))
                    {
                        VisibleControls.Remove(_penConfig);
                        return;
                    }

                    if (VisibleControls.Contains(_colorSelection))
                        VisibleControls.Remove(_colorSelection);

                    VisibleControls.Add(_penConfig);
                    break;
            }
            OnPropertyChanged(nameof(VisibleControls));
        }

        private void UpdateDrawingAttributes(object sender, Color e)
        {
            _drawingAttributes.Color = e;
            OnPropertyChanged(nameof(DrawingAttributes));
        }

        private void RegisterKeyBindings()
        {
            var showColors = new HotKey(Key.C, ModifierKeys.Alt | ModifierKeys.Shift);

            var showPen = new HotKey(Key.P, ModifierKeys.Alt | ModifierKeys.Shift);

            var showColorsConfigAction = () =>
            {
                ShowUserTool(ModalWindowType.ColorPicker);
            };

            var showPenConfigAction = () =>
            {
                ShowUserTool(ModalWindowType.PenConfig);
            };

            MainWindow.RegisterHotKey(showColors, showColorsConfigAction);
            MainWindow.RegisterHotKey(showPen, showPenConfigAction);
        }

        private void TakeScreenshotInternal()
        {

        }
    }
}
