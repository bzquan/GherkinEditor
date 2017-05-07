using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.IO.Packaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit;
using System.Windows;
using System.Windows.Input;
using System.Windows.Documents;

using Word = NetOffice.WordApi;
using NetOffice.WordApi.Enums;
using System.Windows.Xps;

namespace Gherkin.Model
{
    public class FlowDocumentSaver
    {
        private TextEditor TextEditor { get; set; }

        public FlowDocumentSaver(TextEditor textEditor)
        {
            TextEditor = textEditor;
        }
        
        public string SaveAsPDFByWord()
        {
            string pdfFilePath = ShowSaveAsDialog("PDF files(*.pdf)|*.pdf", ".pdf");
            if (pdfFilePath == null) return null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                string tempRTFfilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".rtf";
                SaveToRTF(tempRTFfilePath);

                ConvertFormatByNetOffice(tempRTFfilePath, pdfFilePath, WdSaveFormat.wdFormatPDF);
                File.Delete(tempRTFfilePath);

                InvalidateWaitCursor();
                return pdfFilePath;
            }
            catch (Exception ex)
            {
                InvalidateWaitCursor();
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public string SaveAsPDFBySharpPDF()
        {
            string pdfFilePath = ShowSaveAsDialog("PDF files(*.pdf)|*.pdf", ".pdf");
            if (pdfFilePath == null) return null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                SaveAsPDFBySharpPDF(pdfFilePath);
                InvalidateWaitCursor();
                return pdfFilePath;
            }
            catch (Exception ex)
            {
                InvalidateWaitCursor();
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void InvalidateWaitCursor() => Mouse.OverrideCursor = null;

        /// <summary>
        /// Create PDF file by using PdfSharp. It create a InMemory XPS file internally,
        /// then convert the XPS stream to PDF file.
        /// </summary>
        /// <param name="pdfFilePath"></param>
        private void SaveAsPDFBySharpPDF(string pdfFilePath)
        {
            using (MemoryStream xpsStream = new MemoryStream())
            {
                Package package = Package.Open(xpsStream, FileMode.Create);
                XpsDocument xpsDocInMemory = new XpsDocument(package);
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xpsDocInMemory);

                DocumentPaginatorWrapper paginator = Printing.CreateDocumentPaginatorToPrint(TextEditor);
                paginator.Title = FilePath2Title(pdfFilePath);

                writer.Write(paginator);
                xpsDocInMemory.Close();
                package.Close();

                var xpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(xpsStream);
                PdfSharp.Xps.XpsConverter.Convert(xpsDoc, pdfFilePath, 0);
            }
        }

        public string SaveAsXPS()
        {
            string xpsFilePath = ShowSaveAsDialog("XPS files(*.xps)|*.xps", ".xps");
            if (xpsFilePath == null) return null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                SaveAsXps(TextEditor, xpsFilePath, FilePath2Title(xpsFilePath));
                InvalidateWaitCursor();
                return xpsFilePath;
            }
            catch (Exception ex)
            {
                InvalidateWaitCursor();
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public string SaveAsRTF()
        {
            string rtfFilePath = ShowSaveAsDialog("RTF files(*.rtf)|*.rtf", ".rtf");
            if (rtfFilePath == null) return null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                SaveToRTF(rtfFilePath);
                InvalidateWaitCursor();
                return rtfFilePath;
            }
            catch (Exception ex)
            {
                InvalidateWaitCursor();
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void SaveToRTF(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                var flowDocument = Printing.CreateFlowDocumentToPrint(TextEditor);
                var textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                textRange.Save(fileStream, DataFormats.Rtf);
            }
        }
        
        public string SaveAsDocx()
        {
            string docxFilePath = ShowSaveAsDialog("Word files(*.docx)|*.docx", ".docx");
            if (docxFilePath == null) return null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                string tempRTFfilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".rtf";
                SaveToRTF(tempRTFfilePath);

                ConvertFormatByNetOffice(tempRTFfilePath, docxFilePath, WdSaveFormat.wdFormatDocumentDefault);
                File.Delete(tempRTFfilePath);

                InvalidateWaitCursor();
                return docxFilePath;
            }
            catch (Exception ex)
            {
                InvalidateWaitCursor();
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Convert to docx or PDF by using "NetOffice - MS Office in .NET", which uses Word application internal
        /// without version limitations.
        /// Note:
        /// You need to add NetOffice.dll and WordApi.dll "References".
        /// To avoid error CS1752: Interop type 'Application' cannot be embedded. Use the applicable interface instead,
        /// in your Project, expand the "References", find the NetOffice and WordApi reference.
        /// Right click it and select properties, and change "Embed Interop Types" to false.
        /// </summary>
        /// <param name="fromFilePath"></param>
        /// <param name="toFilePath"></param>
        /// <param name="format"></param>
        private static void ConvertFormatByNetOffice(string fromFilePath, string toFilePath, WdSaveFormat format)
        {
            Word.Application wordApplication = new Word.Application()
            {
                DisplayAlerts = WdAlertLevel.wdAlertsNone
            };
            var currentDoc = wordApplication.Documents.Open(fromFilePath);
            currentDoc.SaveAs(toFilePath, format);

            currentDoc.Close();
            wordApplication.Quit();
        }


        private string FilePath2Title(string filePath) => Path.GetFileName(filePath);

        private string ShowSaveAsDialog(string filter, string extension)
        {
            string defaultFilePath = CreateDefaultFilePath(extension);

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = defaultFilePath;
            dlg.DefaultExt = extension;
            dlg.FilterIndex = 1;
            dlg.Filter = filter + "|All Files (*.*)|*.*";
            var result = dlg.ShowDialog();

            return (result == true) ? dlg.FileName : null;
        }

        private string CreateDefaultFilePath(string ext)
        {
            string defaultFilePath = TextEditor.Document.FileName;
            string originalExt = Path.GetExtension(defaultFilePath);
            if (!string.IsNullOrEmpty(originalExt))
                return defaultFilePath.Replace(originalExt, ext);
            else
                return defaultFilePath + ext;
        }

        private void SaveAsXps(TextEditor textEditor, string fileName, string title)
        {
            using (Package container = Package.Open(fileName, FileMode.Create))
            {
                using (XpsDocument xpsDoc = new XpsDocument(container, CompressionOption.Fast))
                {
                    XpsSerializationManager xpsSerializationManager = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    DocumentPaginatorWrapper paginator = Printing.CreateDocumentPaginatorToPrint(textEditor);
                    paginator.Title = title;

                    xpsSerializationManager.SaveAsXaml(paginator);
                }
            }
        }
    }
}
