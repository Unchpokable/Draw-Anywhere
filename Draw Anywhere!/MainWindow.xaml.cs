using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
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
            _lastActions = new Stack<StrokeCollection>();

            Deactivated += OnFocusLost;
            RegisterDefaultHotKeys();
            Topmost = true;

            DrawField.Strokes.StrokesChanged += HandleStrokesChanged;

            FitWindowSizeAndPositionToActiveMonitor();

            ((MainViewModel)DataContext).UndoRequested += (s, e) => UndoStrokes();
            ((MainViewModel)DataContext).HideRequested += (s, e) => HideOverlay();
        }

        ~MainWindow()
        {
            _applicationHotKeyManager.Dispose();
        }

        private HotKey _showHotKey;

        private readonly HotKeyManager _applicationHotKeyManager;

        private Stack<StrokeCollection> _lastActions;

        private bool _handleDrawAction = true;

        private void RegisterDefaultHotKeys()
        {
            //_hideHotKey = new HotKey(Key.Escape, ModifierKeys.None);
            //_applicationHotKeyManager.Register(_hideHotKey);

            _showHotKey = new HotKey(Key.D, ModifierKeys.Shift | ModifierKeys.Alt);
            _applicationHotKeyManager.Register(_showHotKey);

            //_undoHotKey = new HotKey(Key.Z, ModifierKeys.Control);
            //_applicationHotKeyManager.Register(_undoHotKey);

            _applicationHotKeyManager.KeyPressed += HandleKeyPressed;
        }

        private void HandleKeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Equals(_showHotKey))
            {
                _lastActions.Clear();
                DrawField.Strokes.Clear();
                FitWindowSizeAndPositionToActiveMonitor();
                Show();
                Focus();
            }
        }

        private void HideOverlay()
        {
            Hide();
        }

        private void OnFocusLost(object sender, EventArgs e)
        {
            Hide();
        }

        private void UndoStrokes()
        {
            if (_lastActions.Count <= 0)
                return;

            _handleDrawAction = false;
            DrawField.Strokes.Remove(_lastActions.Pop());
            _handleDrawAction = true;
        }

        private void HandleStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (_handleDrawAction) 
                _lastActions.Push(e.Added);
        }

        private void FitWindowSizeAndPositionToActiveMonitor()
        {
            if (WinApi.GetCursorPos(out var cursorPosition))
            {
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
        }
    }
}
