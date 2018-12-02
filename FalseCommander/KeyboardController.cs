using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FalseCommander {
    class KeyboardController {

        public static KeyboardController instance;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static ActionK callbackDown;
        private static ActionK callbackUp;

        public KeyboardController (ActionK _callbackDown, ActionK _callbackUp) {

            callbackDown = _callbackDown;
            callbackUp = _callbackUp;
            if (instance != null)
                return;

            instance = this;

            _hookID = SetHook (_proc);
        }

        public void Destroy () {

            UnhookWindowsHookEx (_hookID);
        }

        private static IntPtr SetHook (LowLevelKeyboardProc proc) {
            using (Process curProcess = Process.GetCurrentProcess ())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx (WH_KEYBOARD_LL, proc,
                    GetModuleHandle (curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc (
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback (
            int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN) {
                int vkCode = Marshal.ReadInt32 (lParam);
                callbackDown ((Keys) vkCode);
            }

            if (nCode >= 0 && wParam == (IntPtr) WM_KEYUP) {
                int vkCode = Marshal.ReadInt32 (lParam);
                callbackUp ((Keys) vkCode);
            }

            return CallNextHookEx (_hookID, nCode, wParam, lParam);
        }

        [DllImport ("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx (int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport ("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs (UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx (IntPtr hhk);

        [DllImport ("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx (IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport ("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle (string lpModuleName);


    }
}
