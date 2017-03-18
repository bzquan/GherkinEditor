using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace Gherkin.Util
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FilePathStruct
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string FilePath;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CopyDataStruct
    {
        public IntPtr dwData;       // Specifies data to be passed
        public int cbData;          // Specifies the data size in bytes
        public IntPtr lpData;       // Pointer to data to be passed
    }

    [SuppressUnmanagedCodeSecurity]
    public class NativeMethod
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg,
            IntPtr wParam, ref CopyDataStruct lParam);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }

    public class ProcessInterop
    {
        public const int WM_COPYDATA = 0x004A;
        public const string GherkinInteropMessageReceiver = "GherkinInteropMessageReceiver";　 // It should be used as Window Title that will receive message

        public static bool SendFilePathToApp(string filePath)
        {
            IntPtr hTargetWnd = NativeMethod.FindWindow(null, GherkinInteropMessageReceiver);
            if (hTargetWnd == IntPtr.Zero)
            {
                //MessageBox.Show("Failed - NativeMethod.FindWindow(null, \"GherkinInteropMessageReceiver\")");
                return false;
            }
            FilePathStruct filePathStruct;
            filePathStruct.FilePath = filePath;
            int size_of_struct = Marshal.SizeOf(filePathStruct);
            IntPtr pStruct = Marshal.AllocHGlobal(size_of_struct);
            try
            {
                Marshal.StructureToPtr(filePathStruct, pStruct, true);

                CopyDataStruct cds = new CopyDataStruct();
                cds.cbData = size_of_struct;
                cds.lpData = pStruct;
                NativeMethod.SendMessage(hTargetWnd, ProcessInterop.WM_COPYDATA, new IntPtr(), ref cds);

                Marshal.GetLastWin32Error();
            }
            finally
            {
                Marshal.FreeHGlobal(pStruct);
            }
            return true;
        }
    }
}
