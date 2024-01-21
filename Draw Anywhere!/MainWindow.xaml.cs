using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using DrawAnywhere.Sys;
using GlobalHotKey;

namespace DrawAnywhere
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static MainWindow()
        {
            ApplicationHotKeyManager = new HotKeyManager();
            _externalBindings = new List<Action<HotKey>>();
            _externalKeys = new List<HotKey>();
        }

        public MainWindow()
        {
            InitializeComponent();
            _lastActions = new Stack<StrokeCollection>();
            _showOverlay = true;

            Deactivated += OnFocusLost;
            RegisterDefaultHotKeys();
            Topmost = true;

            DrawField.Strokes.StrokesChanged += HandleStrokesChanged;

            FitWindowSizeAndPositionToActiveMonitor();
        }

        ~MainWindow()
        {
            ApplicationHotKeyManager.Dispose();
        }

        private HotKey _hideHotKey;
        private HotKey _showHotKey;
        private HotKey _undoHotKey;

        public static readonly HotKeyManager ApplicationHotKeyManager;

        private Stack<StrokeCollection> _lastActions;

        private bool _handleDrawAction = true;

        private static List<Action<HotKey>> _externalBindings;
        private static List<HotKey> _externalKeys;

        private static bool _showOverlay;

        public static void RegisterHotKey(HotKey key, Action execute)
        {
            Action<HotKey> executeBound = hotKey =>
            {
                if (hotKey.Equals(key))
                    execute();
            };

            _externalKeys.Add(key);
            ApplicationHotKeyManager.Register(key);
            _externalBindings.Add(executeBound);
        }

        private void RegisterDefaultHotKeys()
        {
            _hideHotKey = new HotKey(Key.Escape, ModifierKeys.None);
            ApplicationHotKeyManager.Register(_hideHotKey);

            _showHotKey = new HotKey(Key.D, ModifierKeys.Shift | ModifierKeys.Alt);
            ApplicationHotKeyManager.Register(_showHotKey);

            _undoHotKey = new HotKey(Key.Z, ModifierKeys.Control);
            ApplicationHotKeyManager.Register(_undoHotKey);

            ApplicationHotKeyManager.KeyPressed += HandleKeyPressed;
        }

        private void HandleKeyPressed(object sender, KeyPressedEventArgs e)
        {
            //Hide();
            if (_showOverlay)
            {
                var dbg = _externalBindings;
                foreach (var bind in _externalBindings)
                {
                    bind?.Invoke(e.HotKey);
                }
            }

            if (e.HotKey.Equals(_hideHotKey) || e.HotKey.Key == Key.Escape)
            {
                _lastActions.Clear();
                DrawField.Strokes.Clear();
                Hide();
                _showOverlay = false;

                // Unregistering all external hotkeys to allow user use his keyboard
                foreach (var key in _externalKeys) 
                    ApplicationHotKeyManager.Unregister(key);
            }

            if (e.HotKey.Equals(_showHotKey))
            {
                FitWindowSizeAndPositionToActiveMonitor();
                Show();
                Focus();

                if(!_showOverlay)
                    foreach (var key in _externalKeys)
                        ApplicationHotKeyManager.Register(key);
                
                _showOverlay = true;
            }

            if (e.HotKey.Equals(_undoHotKey))
            {
                UndoStrokes();
            }
        }
        
        private void OnFocusLost(object sender, EventArgs e)
        {
            Hide();
            _showOverlay = false;
            // Unregistering all external hotkeys to allow user use his keyboard
            foreach (var key in _externalKeys)
                ApplicationHotKeyManager.Unregister(key);
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
