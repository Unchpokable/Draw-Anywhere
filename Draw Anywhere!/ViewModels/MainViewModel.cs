﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using DrawAnywhere.Models;
using DrawAnywhere.MvvmCore;
using DrawAnywhere.Sys;
using DrawAnywhere.ViewModels.Helpers;
using MaterialDesignThemes.Wpf;

namespace DrawAnywhere.ViewModels
{
    internal class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            _config = AppConfig.Instance();

            _penColor = new ByRef<Color>(Colors.AliceBlue);
            _penColor.ValueChanged += UpdateDrawingAttributes;

            DrawingAttributes = new DrawingAttributes
                { Color = _penColor.Value, FitToCurve = true, Height = 6, Width = 6 };

            Background = new OpacityColor(_config.CanvasBackgroundColor, _config.CanvasBackgroundOpacity);
            CurrentInputModeIcon = PackIconKind.Draw;

            InitializeUiComponentsCollections();
            InitializeViewModels();
            InitializeCommands();
        }

        public event EventHandler UndoRequested;
        public event EventHandler HideRequested;
        public event EventHandler ShowRequested;
        public event EventHandler QuitRequested;
        public event EventHandler CleanupRequested;
        public event EventHandler<InkCanvasEditingMode> EditingModeChangeRequested;

        public StrokeCollection CanvasStrokes { get; set; }

        public RelayCommand Quit { get; set; }
        public RelayCommand ShowOverlay { get; set; }
        public RelayCommand HideUi { get; set; }
        public RelayCommand HideOverlay { get; set; }
        public RelayCommand CallUndo { get; set; }
        public RelayCommand ClearCanvas { get; set; }
        public RelayCommand ShowTool { get; set; }
        public RelayCommand MakeScreenShot { get; set; }
        public RelayCommand ChangeEditingMode { get; set; }

        public ObservableCollection<ObservableObject> VisibleControls { get; set; }

        public ObservableCollection<ObservableObject> ChildControls { get; set; }

        public PackIconKind CurrentInputModeIcon
        {
            get => _inputModeIcon;
            set
            {
                _inputModeIcon = value;
                OnPropertyChanged();
            }
        }

        public OpacityColor Background
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        public DrawingAttributes DrawingAttributes
        {
            get => _drawingAttributes;
            set
            {
                _drawingAttributes = value;
                OnPropertyChanged();
            }
        }

        private PackIconKind _inputModeIcon;

        private DrawingAttributes _drawingAttributes;
        private OpacityColor _backgroundColor;

        private ColorSelectionViewModel _colorSelection;
        private PenConfigViewModel _penConfig;
        private CanvasParamsViewModel _canvasConfig;
        private SettingsViewModel _settings;

        private ByRef<Color> _penColor;

        private AppConfig _config;

        private void InitializeUiComponentsCollections()
        {
            VisibleControls = new ObservableCollection<ObservableObject>();
            ChildControls = new ObservableCollection<ObservableObject>();

            ChildControls.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(ChildControls)); };
            VisibleControls.CollectionChanged += (s, e) => { OnPropertyChanged(nameof(VisibleControls)); };
        }

        private void InitializeViewModels()
        {
            _colorSelection = new ColorSelectionViewModel(_penColor);
            _penConfig = new PenConfigViewModel(DrawingAttributes);
            _canvasConfig = new CanvasParamsViewModel(this, Background, _config);
            _settings = new SettingsViewModel(this);

            _penConfig.PropertyChanged += (s, e) => OnPropertyChanged(nameof(DrawingAttributes));
        }

        private void InitializeCommands()
        {
            ShowTool = new RelayCommand(ShowUserTool);
            MakeScreenShot = new RelayCommand(CaptureScreenshot);
            CallUndo = new RelayCommand(OnUndoCalled);
            HideOverlay = new RelayCommand(OnHideCalled);
            HideUi = new RelayCommand(HideUiComponents);
            ShowOverlay = new RelayCommand(OnOverlayCalled);
            Quit = new RelayCommand(OnQuitRequested);
            ClearCanvas = new RelayCommand(OnClearRequested);
            ChangeEditingMode = new RelayCommand(ChangeEditingModeInternal);
        }

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

        private void ChangeEditingModeInternal(object parameter)
        {
            if (parameter is not InkCanvasEditingMode mode)
                return;

            switch (mode)
            {
                case InkCanvasEditingMode.Ink:
                    CurrentInputModeIcon = PackIconKind.Draw;
                    break;
                case InkCanvasEditingMode.EraseByPoint:
                    CurrentInputModeIcon = PackIconKind.EraserVariant;
                    break;
                case InkCanvasEditingMode.EraseByStroke:
                    CurrentInputModeIcon = PackIconKind.Eraser;
                    break;
            }


            EditingModeChangeRequested?.Invoke(this, mode);
        }

        private void ShowUserTool(object parameter)
        {
            if (parameter is not ModalWindowType toolType)
                return;

            _settings.CloseAllDialogs(); // Settings is the only control that can create its own child windows
            _canvasConfig.CloseDialog();

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

                case ModalWindowType.CanvasConfig:
                    selectedControl = _canvasConfig;
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
            if (VisibleControls.Count > 0)
            {
                _settings.CloseAllDialogs(); // Settings is the only control that can create its own child windows
                _canvasConfig.CloseDialog();

                HideUiComponents(new());
                return;
            }

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

        private void OnClearRequested(object _)
        {
            CleanupRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnCanvasStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            HideUiComponents(new());
        }
    }
}
