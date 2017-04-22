using Gherkin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Gherkin.View
{
    /// <summary>
    /// Interaction logic for InteropMessageReceiver.xaml
    /// </summary>
    public partial class InteropMessageReceiver : Window
    {
        private ViewModel.GherkinViewModel m_ViewModel;

        public InteropMessageReceiver(Window parent, ViewModel.GherkinViewModel viewModel)
        {
            InitializeComponent();

            this.Owner = parent;
            m_ViewModel = viewModel;
            this.Title = ProcessInterop.GherkinInteropMessageReceiver;
        }

        /// <summary>
        /// Initialize window to register WndProc but do not show it
        /// because it is only a message receiver
        /// </summary>
        public void InitHwnd()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();

            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            System.Windows.Forms.Message m = System.Windows.Forms.Message.Create(hwnd, msg, wParam, lParam);
            if (m.Msg != ProcessInterop.WM_COPYDATA) return IntPtr.Zero;

            // Get the CopyDataStruct struct from lParam.
            CopyDataStruct cds = (CopyDataStruct)m.GetLParam(typeof(CopyDataStruct));
            if (cds.cbData == Marshal.SizeOf(typeof(FilePathStruct)))　// If the size matches
            {
                // Marshal the data from the unmanaged memory block to a FilePathStruct managed struct.
                FilePathStruct filePathStruct = (FilePathStruct)Marshal.PtrToStructure(cds.lpData, typeof(FilePathStruct));

                OpenFile(filePathStruct.FilePath);
                if (this.Owner.WindowState == WindowState.Minimized)
                {
                    this.Owner.WindowState = WindowState.Normal;
                }
                this.Owner.Activate(); // Bring the Gherkin window to the foreground and activates it.
            }
            return IntPtr.Zero;
        }

        private void OpenFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                m_ViewModel.OpenFiles(filePath);
            }
        }
    }
}
