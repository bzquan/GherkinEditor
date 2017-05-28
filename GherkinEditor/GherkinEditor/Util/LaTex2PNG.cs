using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = NetOffice.WordApi;
using NetOffice.WordApi.Enums;
using NetOffice.WordApi;
using System.Windows.Media.Imaging;

namespace Gherkin.Util
{
    public class LaTex2PNG : IDisposable
    {
        private Application _wordApp;
        private Document _doc;
        private Range _range;

        private string _saveName;
        private string _extractPath;

        public LaTex2PNG()
        {
            _wordApp = new Word.Application()
            {
                DisplayAlerts = WdAlertLevel.wdAlertsNone
            };

            Guid _guid = Guid.NewGuid();
            string guidToString = _guid.ToString("N");
            string saveNameBase = System.IO.Path.Combine(System.IO.Path.GetTempPath(), guidToString);
            _saveName = saveNameBase + ".html";
            _extractPath = saveNameBase + @".files\image001.png";

            _wordApp.Visible = false;
            _doc = _wordApp.Documents.Add();
            _range = _doc.Range();
            _range.Text = "5";
            _doc.OMaths.Add(_range);
        }

        public BitmapImage ToBitmapImage(string latex)
        {
            foreach (var ac in _wordApp.OMathAutoCorrect.Entries)
            {
                if (latex.Contains(ac.Name))
                {
                    latex = latex.Replace(ac.Name, ac.Value);
                }
            }

            _range.Text = latex;
            _doc.OMaths.BuildUp();
            _doc.SaveAs(_saveName, WdSaveFormat.wdFormatHTML, Type.Missing, Type.Missing, false, Type.Missing, null, false);

            return Util.BitmapImageFromFile(_extractPath);
        }

        public void Dispose()
        {
            _range = null;
            _doc = null;
            _wordApp.Documents.Close(WdSaveOptions.wdDoNotSaveChanges);
            _wordApp.Quit(false);
            _wordApp = null;
            System.IO.File.Delete(_saveName);
            for (int i = 0; i < 2; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
