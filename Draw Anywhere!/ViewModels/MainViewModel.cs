using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Sys;
using Color = System.Windows.Media.Color;

namespace DrawAnywhere.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            VisibleControls = new ObservableCollection<ObservableObject>();
            ChildControls = new ObservableCollection<ObservableObject>();

            ChildControls.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(ChildControls)); };
            VisibleControls.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(VisibleControls)); };
            _penColor = new ByRef<Color>(Colors.AliceBlue);
            
            DrawingAttributes = new DrawingAttributes()
                { Color = _penColor.Value, FitToCurve = true, Height = 6, Width = 6 };

            _colorSelection = new ColorSelectionViewModel(_penColor);
            _penConfig = new PenConfigViewModel(DrawingAttributes);
            _settings = new SettingsViewModel(this);

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

        public StrokeCollection CanvasStrokes { get; set; }

        public RelayCommand Quit { get; set; }
        public RelayCommand ShowOverlay { get; set; }
        public RelayCommand HideUi { get; set; }
        public RelayCommand HideOverlay { get; set; }
        public RelayCommand CallUndo { get; set; }
        public RelayCommand ShowTool { get; set; }
        public RelayCommand MakeScreenShot { get; set; }

        public ObservableCollection<ObservableObject> VisibleControls { get; set; }

        public ObservableCollection<ObservableObject> ChildControls { get; set; }

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
        private SettingsViewModel _settings;

        private ByRef<Color> _penColor;

        public void BindCanvasStrokes(StrokeCollection strokes)
        {
            CanvasStrokes = strokes;
            CanvasStrokes.StrokesChanged += OnCanvasStrokesChanged;
        }

        public void AppendControlChild(ObservableObject ctrl)
        {
            if (ctrl == null)
                return;

            ChildControls.Add(ctrl);
            VisibleControls.Add(ctrl);
        }

        public void CloseChild(ObservableObject ctrl)
        {
            if (ctrl == null) 
                return;

            ChildControls.Remove(ctrl); 
            VisibleControls.Remove(ctrl);
        }

        private void ShowUserTool(object parameter)
        {
            if (parameter is not ModalWindowType toolType)
                return;

            _settings.CloseAllDialogs(); // Settings is the only control that can create its own child windows

            foreach (var control in ChildControls)
            {
                VisibleControls.Remove(control);
            }

            ObservableObject selectedControl;

            switch (toolType)
            {
                case ModalWindowType.ColorPicker:
                    selectedControl = _colorSelection;
                    break;

                case ModalWindowType.PenConfig:
                    selectedControl = _penConfig;
                    break;

                case ModalWindowType.Settings:
                    selectedControl = _settings;
                    break;
                default:
                    selectedControl = null;
                    break;
            }

            var currentControl = VisibleControls.FirstOrDefault();

            if (selectedControl == null)
                return;

            VisibleControls.Clear();
            
            if (selectedControl != currentControl)
                VisibleControls.Add(selectedControl);

            OnPropertyChanged(nameof(VisibleControls));
        }

        private void HideUiComponents(object _)
        {
            VisibleControls.Clear();
            OnPropertyChanged(nameof(VisibleControls));
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


        private void OnCanvasStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            HideUiComponents(new());
        }
    }
}
