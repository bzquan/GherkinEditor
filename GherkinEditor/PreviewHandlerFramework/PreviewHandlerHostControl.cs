using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using C4F.DevKit.PreviewHandler.PreviewHandlerFramework;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerHost
{
    /// <summary>
    /// This control is dependent on the managed framework for preview handlers implemented by Stephen Toub
    /// and published in the December 2006 issue of MSDN Magazine.  http://msdn.microsoft.com/msdnmag/issues/07/01/PreviewHandlers/default.aspx
    /// In this article, he implements a managed wrapper to the COM Preview Handler interfaces IPreviewHandler, IInitializeWithFile and IInitializeWithStream
    /// 
    /// In this class, we look up the registered preview handler for a given file extension, using reflection, instantiate an instance of the handler.
    /// We then check if the handler is a Stream or File Handler by checking which interface is implemented.  We then initialize the handler, pass a handle to 
    /// our control and the bounds of our control, and call DoPreview (part of the IPreviewHandler interface)
    /// 
    /// Developed by Ryan Powers - Clarity Consulting - http://www.claritycon.com
    /// </summary>
    [ToolboxItem(true), ToolboxBitmap(typeof(PreviewHandlerHostControl))]
    public partial class PreviewHandlerHostControl : UserControl
    {
        private string _filePath;
        private object _comInstance = null;
        private Stream _fileStream = null;

        public PreviewHandlerHostControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Full path to file to be previewed
        /// 
        /// Whenever a new path is set, the preview is generated
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
            set { 
                _filePath = value;
                if(value != null && !IsDesignTime())
                {
                    CleanupComInstance();
                    GeneratePreview();
                }
            }
        }

        private bool IsDesignTime()
        {
            return (this.Site != null && this.Site.DesignMode);
        }

        /// <summary>
        /// 1) Look up the preview handler associated with the file extension
        /// 2) Create an instance of the handler using its CLSID and reflection
        /// 3) Check if it is a file or stream handler
        /// 4) Initialize with File or Stream using Initialize from the appropriate interface
        /// 5) Call SetWindow passing in a handle to this control and the bounds of the control
        /// 6) Call DoPreview
        /// </summary>
        private void GeneratePreview()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    ShowMessage("File not found");
                    return;
                }

                PreviewHandlerInfo handler = LookupPreviewHandler();
                if (handler == null)
                {
                    ShowMessage("No Preview Available");
                    return;
                }

                lblMessage.Visible = false;
                CreateComInstance(handler);
                InitializeComInstance();
                SetWindow();
                ((IPreviewHandler)_comInstance).DoPreview();
            }
            catch (Exception ex)
            {
                CleanupComInstance();
                ShowMessage("Preview Generation Failed - " + ex.Message);

                throw ex;
            }
        }

        private void ShowMessage(string message)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
        }

        /// <summary>
        /// Look up the preview handler associated with the file extension
        /// </summary>
        /// <returns></returns>
        private PreviewHandlerInfo LookupPreviewHandler()
        {
            RegistrationData data = PreviewHandlerRegistryAccessor.LoadRegistrationInformation();
            var extension = data.Extensions.FirstOrDefault(x => HasExtension(_filePath, x.Extension) && (x.Handler != null));

            return extension?.Handler;
        }

        private bool HasExtension(string filePath, string ext)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            return filePath.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase);
        }

        private void CreateComInstance(PreviewHandlerInfo handler)
        {
            Type comType = Type.GetTypeFromCLSID(new Guid(handler.ID));
            _comInstance = Activator.CreateInstance(comType);
        }

        /// <summary>
        /// Initialize with File or Stream using Initialize from the appropriate interface
        /// </summary>
        private void InitializeComInstance()
        {
            // Check if it is a stream or file handler
            if (_comInstance is IInitializeWithFile)
            {
                ((IInitializeWithFile)_comInstance).Initialize(_filePath, 0);
            }
            else if (_comInstance is IInitializeWithStream)
            {
                _fileStream = File.Open(_filePath, FileMode.Open);
                StreamWrapper stream = new StreamWrapper(_fileStream);
                ((IInitializeWithStream)_comInstance).Initialize(stream, 0);
            }
        }

        private void SetWindow()
        {
            RECT r = new RECT() { top = 0, bottom = this.Height, left = 0, right = this.Width };
            ((IPreviewHandler)_comInstance).SetWindow(this.Handle, ref r);
        }

        public void CleanupComInstance()
        {
            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream = null;
            }

            if (_comInstance != null)
            {
                ((IPreviewHandler)_comInstance).Unload();
                _comInstance = null;
            }
        }

        private void Control_Resize(object sender, EventArgs e)
        {
            if(_comInstance != null)
            {
                RECT r = new RECT() { top = 0, bottom = this.Height, left = 0, right = this.Width };
                ((IPreviewHandler)_comInstance).SetRect(ref r);
            }
        }
    }
}