using Gherkin.Model;
using Gherkin.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace Gherkin.View
{
    /// <summary>
    /// Represents the PrintPreviewDialog class to preview documents 
    /// of type FlowDocument, IDocumentPaginatorSource or DocumentPaginatorWrapper
    /// using the PrintPreviewDocumentViewer class.
    /// </summary>
    public partial class PrintPreviewDialog : Window
    {
        private object m_Document;

        public PrintPreviewDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the document viewer.
        /// </summary>
        public PrintPreviewDocumentViewer DocumentViewer
        {
            get { return documentViewer; }
            set { documentViewer = value; }
        }

        /// <summary>
        /// Loads the specified FlowDocument document for print preview.
        /// </summary>
        public void LoadDocument(FlowDocument document)
        {
            m_Document = document;
            string temp = System.IO.Path.GetTempFileName();

            if (File.Exists(temp) == true)
                File.Delete(temp);

            XpsDocument xpsDoc = new XpsDocument(temp, FileAccess.ReadWrite);
            XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
            xpsWriter.Write(((FlowDocument)document as IDocumentPaginatorSource).DocumentPaginator);
            documentViewer.Document = xpsDoc.GetFixedDocumentSequence();
            xpsDoc.Close();
        }

        /// <summary>
        /// Loads the specified DocumentPaginatorWrapper document for print preview.
        /// </summary>
        public void LoadDocument(DocumentPaginatorWrapper document)
        {
            m_Document = document;
            string temp = System.IO.Path.GetTempFileName();

            if (File.Exists(temp) == true)
                File.Delete(temp);

            XpsDocument xpsDoc = new XpsDocument(temp, FileAccess.ReadWrite);
            XpsDocumentWriter xpsWriter = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
            xpsWriter.Write(document);
            documentViewer.Document = xpsDoc.GetFixedDocumentSequence();
            xpsDoc.Close();
        }

        /// <summary>
        /// Loads the specified IDocumentPaginatorSource document for print preview.
        /// </summary>
        public void LoadDocument(IDocumentPaginatorSource document)
        {
            m_Document = document;
            documentViewer.Document = document;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
 }
