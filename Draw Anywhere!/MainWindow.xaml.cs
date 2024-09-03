using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using DrawAnywhere.Models;
using DrawAnywhere.Sys;
using DrawAnywhere.ViewModels;
using GlobalHotKey;

namespace DrawAnywhere
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            _applicationHotKeyManager = new HotKeyManager();
            InitializeComponent();
            _config = AppConfig.Instance();
            _lastActions = new Stack<StrokeCollection>();

            Deactivated += OnFocusLost;
            RegisterDefaultHotKeys();
            Topmost = true;

            DrawField.Strokes.StrokesChanged += HandleStrokesChanged;

            FitWindowSizeAndPositionToActiveMonitor();

            ((MainViewModel)DataContext).UndoRequested += (s, e) => UndoStrokes();
            ((MainViewModel)DataContext).HideRequested += (s, e) => HideOverlay();
            ((MainViewModel)DataContext).ShowRequested += (s, e) => ShowOverlay();
            ((MainViewModel)DataContext).CleanupRequested += (s, e) => ClearStrokes();
            ((MainViewModel)DataContext).QuitRequested += (s, e) => Application.Current.Shutdown();
            ((MainViewModel)DataContext).EditingModeChangeRequested += OnChangeEditingModeChangeRequested;
            ((MainViewModel)DataContext).BindCanvasStrokes(DrawField.Strokes);

            DrawAnywhereTrayIcon.Icon = Properties.Resources.pen;
#if !DEBUG
            Hide();
#endif
        }

        ~MainWindow()
        {
            _applicationHotKeyManager.Dispose();
        }

        private HotKey _showHotKey;

        private readonly HotKeyManager _applicationHotKeyManager;

        private Stack<StrokeCollection> _lastActions;

        private bool _handleDrawAction = true;

        private AppConfig _config;

        private void RegisterDefaultHotKeys()
        {

            _showHotKey = new HotKey(Key.D, ModifierKeys.Shift | ModifierKeys.Alt);
            _applicationHotKeyManager.Register(_showHotKey);

            _applicationHotKeyManager.KeyPressed += HandleKeyPressed;
        }

        private void HandleKeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Equals(_showHotKey))
            {
                ShowOverlay();
            }
        }

        private void HideOverlay()
        {
            if (_config.CleanCanvasWhenHide)
            {
                ClearStrokes();
            }
            new Thread(() =>
            {
                Thread.Sleep(50);
                Dispatcher.Invoke(Hide);
            }).Start();
        }

        private void OnFocusLost(object sender, EventArgs e)
        {
            HideOverlay();
        }

        private void ShowOverlay()
        {
            FitWindowSizeAndPositionToActiveMonitor();
            Show();
            Focus();
            DrawField.Focus();
        }

        private void UndoStrokes()
        {
            if (_lastActions.Count <= 0)
                return;

            _handleDrawAction = false;
            DrawField.Strokes.Remove(_lastActions.Pop());
            _handleDrawAction = true;
        }

        private void OnChangeEditingModeChangeRequested(object sender, InkCanvasEditingMode mode)
        {
            DrawField.EditingMode = mode;
        }

        private void HandleStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (_handleDrawAction) 
                _lastActions.Push(e.Added);
        }

        private void FitWindowSizeAndPositionToActiveMonitor()
        {
            if (!WinApi.GetCursorPos(out var cursorPosition)) return;

            IntPtr monitorHandle = WinApi.MonitorFromPoint(cursorPosition, 2); // 2 = MONITOR_DEFAULTTONEAREST

            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            if (WinApi.GetMonitorInfo(monitorHandle, ref monitorInfo))
            {
                var monitorRect = monitorInfo.rcMonitor;
                Left = monitorRect.left;
                Top = monitorRect.top;
                Width = monitorRect.right - monitorRect.left;
                Height = monitorRect.bottom - monitorRect.top;

                Topmost = true;
            }
        }

        private void ClearStrokes()
        {
            DrawField.Strokes.Clear();
            _lastActions.Clear();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            DrawField.Focus();
        }
    }
}
