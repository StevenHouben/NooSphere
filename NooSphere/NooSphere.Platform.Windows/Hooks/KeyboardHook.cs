using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using NooSphere.Platform.Windows.Interopt;
using NooSphere.Platform.Windows.Windowing;
using NooSphere.Platform.Windows.InteroptServices;

namespace NooSphere.Platform.Windows.Hooks
{
    public class KeyboardHook
    {
        #region Private Members
        private static IntPtr hHook;
        private HookEvents.HookProc mouseHookProcedure;
        private bool isHooked = false;
        const int WM_HOTKEY = 0x312;
        #endregion

        #region Private Members
        private NativeWindowEx hotKeyWin;
        #endregion

        #region Events
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
        #endregion

        #region Constructor
        public KeyboardHook()
        {
            hotKeyWin = new NativeWindowEx();
            hotKeyWin.CreateHandle(new System.Windows.Forms.CreateParams());
            hotKeyWin.MessageRecieved += new NativeWindowEx.MessageRecievedEventHandler(hotKeyWin_MessageRecieved);
        }
        #endregion

        #region Private Methods
        private void hotKeyWin_MessageRecieved(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                if (KeyPressed != null)
                    KeyPressed(this, new KeyPressedEventArgs(modifier, key));
            }
        }
        #endregion

        #region Public Methods
        public void RegisterHotKey(int id, ModifierKeys modifiers, Keys keyCode)
        {
            if (id < 0 | id > 0xBFFF)
                new ArgumentException("Key code out of range. Range from O to 0xBFFF");
            if (modifiers == 0)
                new ArgumentException("You need at least one modifier key");
            if (User32.RegisterHotKey(hotKeyWin.Handle, id, (uint)modifiers, (uint)keyCode) == false)
            {
                throw new Win32Exception();
            }
        }
        public bool TryRegisterHotKey(int id, ModifierKeys modifiers, Keys keyCode)
        {
            return User32.RegisterHotKey(hotKeyWin.Handle, id, (uint)modifiers, (uint)keyCode);
        }
        public void UnregisterHotKey(int id)
        {
            if (User32.UnregisterHotKey(hotKeyWin.Handle, id) == false)
                new Win32Exception();
        }
        public bool TryUnregisterHotKey(int id)
        {
            return User32.UnregisterHotKey(hotKeyWin.Handle, id);
        }
        #endregion
    }
    public enum ModifierKeys
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }
    public class KeyPressedEventArgs : EventArgs
    {
        private ModifierKeys modifier;
        private Keys key;
        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
        {
            this.modifier = modifier;
            this.key = key;
        }
        public ModifierKeys Modifier
        {
            get { return modifier; }
        }
        public Keys Key
        {
            get { return key; }
        }
    }
}
