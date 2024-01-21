using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Sys;
using GlobalHotKey;
using Color = System.Windows.Media.Color;
using Hardcodet.Wpf.TaskbarNotification;

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
            MakeScreenShot = new RelayCommand(CaptureScreenshot);
            CallUndo = new RelayCommand(OnUndoCalled);
            HideOverlay = new RelayCommand(OnHideCalled);
            HideUi = new RelayCommand(HideUiComponents);
            ShowOverlay = new RelayCommand(OnOverlayCalled);
            Quit = new RelayCommand(OnQuitRequested);

            _penColor.ValueChanged += UpdateDrawingAttributes;
        }

        public event EventHandler UndoRequested;
        public event EventHandler HideRequested;
        public event EventHandler ShowRequested;
        public event EventHandler QuitRequested;

        public RelayCommand Quit { get; set; }
        public RelayCommand ShowOverlay { get; set; }
        public RelayCommand HideUi { get; set; }
        public RelayCommand HideOverlay { get; set; }
        public RelayCommand CallUndo { get; set; }
        public RelayCommand ShowTool { get; set; }
        public RelayCommand MakeScreenShot { get; set; }

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

        private void HideUiComponents(object _)
        {
            VisibleControls.Remove(_colorSelection);
            VisibleControls.Remove(_penConfig);
        }

        private void UpdateDrawingAttributes(object sender, Color e)
        {
            _drawingAttributes.Color = e;
            OnPropertyChanged(nameof(DrawingAttributes));
        }

        private void CaptureScreenshot(object _)
        {
            ScreenCapture.MakeScreenshot(true, true);
        }

        private void OnUndoCalled(object _)
        {
            UndoRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnHideCalled(object _)
        {
            HideRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnOverlayCalled(object _)
        {
            ShowRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnQuitRequested(object _)
        {
            QuitRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
