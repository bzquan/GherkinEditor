using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Gherkin.View
{
    /// <summary>
    /// Represents the PrintPreviewDocumentViewer class with PrintQueue and
    /// PrintTicket properties for the document viewer.
    /// </summary>
    public class PrintPreviewDocumentViewer : DocumentViewer
    {
        private PrintQueue m_PrintQueue = LocalPrintServer.GetDefaultPrintQueue();
        private PrintTicket m_PrintTicket;

        /// <summary>
        /// Gets or sets the print queue manager.
        /// </summary>
        public PrintQueue PrintQueue
        {
            get { return m_PrintQueue; }
            set { m_PrintQueue = value; }
        }

        /// <summary>
        /// Gets or sets the print settings for the print job.
        /// </summary>
        public PrintTicket PrintTicket
        {
            get { return m_PrintTicket; }
            set { m_PrintTicket = value; }
        }

        protected override void OnPrintCommand()
        {
            // get a print dialog, defaulted to default printer and default printer's preferences.
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintQueue = m_PrintQueue;
            printDialog.PrintTicket = m_PrintTicket;

            if (printDialog.ShowDialog() == true)
            {
                m_PrintQueue = printDialog.PrintQueue;
                m_PrintTicket = printDialog.PrintTicket;
                printDialog.PrintDocument(this.Document.DocumentPaginator, "PrintPreviewJob");
            }
        }
    }
}
