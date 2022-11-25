using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace pscommander
{
    public class HotKeyService 
    {
        private readonly PowerShellService _powerShellService;
        private readonly MenuService _menuService;
        private readonly KeyboardHook _keyboardHook;
        private readonly List<HotKey> _hotKeys; 

        public HotKeyService(PowerShellService powerShellService, MenuService menuService)
        {
            _powerShellService = powerShellService;
            _menuService = menuService;
            _keyboardHook = new KeyboardHook();
            _hotKeys = new List<HotKey>();
            _keyboardHook.KeyPressed += (s, e) => {
                try {
                    uint process = 0;
                    var foregroundWin = UnmanagedMethods.GetForegroundWindow();
                    if (foregroundWin != IntPtr.Zero)
                    {
                        UnmanagedMethods.GetWindowThreadProcessId(foregroundWin, out uint processId);
                        process = processId;
                    }

                    var trigger = _hotKeys.FirstOrDefault(m => m.Keys == e.Key && m.ModifierKeys == e.Modifier);
                    if (trigger == null)
                    {
                        _menuService.ShowError("Hot key not found.");
                        return;
                    }

                    try
                    {
                        _powerShellService.Execute(trigger.Action, process);
                    }
                    catch (Exception ex)
                    {
                        _menuService.ShowError(ex.Message);
                    }
                }
                catch {}
            };
        }

        public void SetHotKeys(IEnumerable<HotKey> hotKeys)
        {
            foreach(var hook in _hotKeys)
            {
                _keyboardHook.UnregisterHotKey(hook.Id);
            }

            _hotKeys.Clear();

            foreach(var hotKey in hotKeys)
            {
                _keyboardHook.RegisterHotKey(hotKey.ModifierKeys, hotKey.Keys);
            }

            _hotKeys.AddRange(hotKeys);
        }
    }

    /// <summary>
    /// Represents the window that is used internally to get the messages.
    /// </summary>
    public class KeyboardHookWindow : NativeWindow
    {
        private static int WM_HOTKEY = 0x0312;
        public KeyboardHookWindow() : base(UnmanagedMethods.WS_POPUP)
        {
        }

        /// <summary>
        /// Overridden to get the notifications.
        /// </summary>
        /// <param name="m"></param>
        protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            // check if we got a hot key pressed.
            if (msg == WM_HOTKEY)
            {
                // get the keys.
                Keys key = (Keys)(((int)lParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)lParam & 0xFFFF);

                // invoke the event to notify the parent.
                if (KeyPressed != null)
                    KeyPressed(this, new KeyPressedEventArgs(modifier, key));
            }
            else
            {
                return base.WndProc(hWnd, msg, wParam, lParam);
            }

            return IntPtr.Zero;
        }

        public event EventHandler<KeyPressedEventArgs> KeyPressed;
    }


    public sealed class KeyboardHook : IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private KeyboardHookWindow _window = new KeyboardHookWindow();
        private static int _currentId;

        public KeyboardHook()
        {
            // register the event of the inner native window.
            _window.KeyPressed += delegate(object sender, KeyPressedEventArgs args)
            {
                if (KeyPressed != null)
                    KeyPressed(this, args);
            };
        }

        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            // increment the counter.
            _currentId = _currentId + 1;

            // register the hot key.
            if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
            {
                //throw new Win32Exception();
                //throw new InvalidOperationException("Couldn’t register the hot key.");
            }
                
        }

        public void UnregisterHotKey(int id)
        {
            if (!UnregisterHotKey(_window.Handle, id))
            {
                //throw new Win32Exception();
            }
        }

        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            // unregister all the registered hot keys.
            for (int i = _currentId; i > 0; i--)
            {
                if (!UnregisterHotKey(_window.Handle, i))
                {
                    //throw new Win32Exception();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys _modifier;
        private Keys _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier
        {
            get { return _modifier; }
        }

        public Keys Key
        {
            get { return _key; }
        }
    }
}