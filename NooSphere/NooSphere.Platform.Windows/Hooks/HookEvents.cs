using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Platform.Windows.Hooks
{
    public static class HookEvents
    {
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
    public enum HookType:int
    {
        WH_MIN              = -1,
        WH_MSGFILTER        = -1,
        WH_JOURNALRECORD    = 0,
        WH_JOURNALPLAYBACK  = 1,
        WH_KEYBOARD         = 2,
        WH_GETMESSAGE       = 3,
        WH_CALLWNDPROC      = 4,
        WH_CBT              = 5,
        WH_SYSMSGFILTER     = 6,
        WH_MOUSE            = 7,
        WH_HARDWARE         = 8,
        WH_DEBUG            = 9,
        WH_SHELL            = 10,
        WH_FOREGROUNDIDLE   = 11,
        WH_CALLWNDPROCRET   = 12,
        WH_KEYBOARD_LL      = 13,
        WH_MOUSE_LL         = 14,
        WH_MAX              = 14
    }
}
