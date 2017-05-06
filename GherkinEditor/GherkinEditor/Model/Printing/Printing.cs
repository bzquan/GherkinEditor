/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Drawing.Printing;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;

#pragma warning disable 618

namespace Gherkin.Model
{
    /// <summary>
    /// Provides Printing Support for AvalonEdit Text Editors
    /// </summary>
    /// <remarks>
    /// Based upon code by Vdue from an AvalonEdit related <a href="http://community.sharpdevelop.net/forums/p/12012/32756.aspx#32756">forum post</a>.
    /// Heavily refactored to support printing in multi-document editors by Rob Vesse
    /// </remarks>
    public static class Printing
    {
        /// <summary>
        /// Invokes a Windows.Forms.PrintPreviewDialog.
        /// </summary>
        public static bool PageSetupDialog(this TextEditor textEditor)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            settings.PageSettings.Landscape = (settings.PrintTicket.PageOrientation == PageOrientation.Landscape);

            var setup = new System.Windows.Forms.PageSetupDialog();
            setup.EnableMetric = true;
            setup.PageSettings = settings.PageSettings;
            var result = setup.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                settings.PageSettings = setup.PageSettings;
                settings.PrintTicket.PageOrientation = (settings.PageSettings.Landscape ? PageOrientation.Landscape : PageOrientation.Portrait);
                settings.PrintTicket.PageMediaSize = ConvertPaperSizeToMediaSize(settings.PageSettings.PaperSize);
            }

            return (result == System.Windows.Forms.DialogResult.OK);
        }

        /// <summary>
        /// Invokes a PrintEngine.PrintPreviewDialog to print preview the TextEditor.Document with specified title.
        /// </summary>
        public static void PrintPreviewDialog(this TextEditor textEditor, string title)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            ShowWaitCursor();
            settings.DocumentTitle = (title != null) ? title : String.Empty;
            Gherkin.View.PrintPreviewDialog printPreview = new Gherkin.View.PrintPreviewDialog();
            printPreview.DocumentViewer.FitToMaxPagesAcross(1);
            printPreview.DocumentViewer.PrintQueue = settings.PrintQueue;

            if (settings.PageSettings.Landscape)
            {
                settings.PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printPreview.DocumentViewer.PrintTicket = settings.PrintTicket;
            printPreview.DocumentViewer.PrintQueue.DefaultPrintTicket.PageOrientation = settings.PrintTicket.PageOrientation;
            printPreview.LoadDocument(CreateDocumentPaginatorToPrint(textEditor));
            
            // this is stupid, but must be done to view a whole page:
            DocumentViewer.FitToMaxPagesAcrossCommand.Execute("1", printPreview.DocumentViewer);
            InvalidateWaitCursor();

            // we never get a return code 'true', since we keep the DocumentViewer open, until user closes the window
            printPreview.ShowDialog();

            settings.PrintQueue = printPreview.DocumentViewer.PrintQueue;
            settings.PrintTicket = printPreview.DocumentViewer.PrintTicket;
        }

        private static void ShowWaitCursor()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        private static void InvalidateWaitCursor() => Mouse.OverrideCursor = null;

        /// <summary>
        /// Invokes a System.Windows.Controls.PrintDialog to print the TextEditor.Document with specified title.
        /// </summary>
        public static void PrintDialog(this TextEditor textEditor, string title)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            ShowWaitCursor();
            settings.DocumentTitle = (title != null) ? title : String.Empty;
            var printDialog = new PrintDialog();
            printDialog.PrintQueue = settings.PrintQueue;

            if (settings.PageSettings.Landscape)
            {
                settings.PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printDialog.PrintTicket = settings.PrintTicket;
            printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = settings.PrintTicket.PageOrientation;

            InvalidateWaitCursor();
            if (printDialog.ShowDialog() == true)
            {
                settings.PrintQueue = printDialog.PrintQueue;
                settings.PrintTicket = printDialog.PrintTicket;
                printDialog.PrintDocument(CreateDocumentPaginatorToPrint(textEditor), "PrintJob");
            }
        }

        /// <summary>
        /// Prints the the TextEditor.Document to the current printer (no dialogs) with specified title.
        /// </summary>
        public static void PrintDirect(this TextEditor textEditor, string title)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            settings.DocumentTitle = (title != null) ? title : String.Empty;
            PrintDialog printDialog = new PrintDialog();
            printDialog.PrintQueue = settings.PrintQueue;

            if (settings.PageSettings.Landscape)
            {
                settings.PrintTicket.PageOrientation = PageOrientation.Landscape;
            }

            printDialog.PrintTicket = settings.PrintTicket;
            printDialog.PrintQueue.DefaultPrintTicket.PageOrientation = settings.PrintTicket.PageOrientation;
            printDialog.PrintDocument(CreateDocumentPaginatorToPrint(textEditor), "PrintDirectJob");
        }

        /// <summary>
        /// Creates a DocumentPaginatorWrapper from TextEditor text to print.
        /// </summary>
        public static DocumentPaginatorWrapper CreateDocumentPaginatorToPrint(TextEditor textEditor)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            // this baby adds headers and footers
            IDocumentPaginatorSource dps = CreateFlowDocumentToPrint(textEditor);
            DocumentPaginatorWrapper dpw = new DocumentPaginatorWrapper(dps.DocumentPaginator, settings.PageSettings, settings.PrintTicket, textEditor.FontFamily);
            dpw.Title = settings.DocumentTitle;
            return dpw;
        }

        /// <summary>
        /// Creates a FlowDocument from TextEditor text to print.
        /// </summary>
        public static FlowDocument CreateFlowDocumentToPrint(TextEditor textEditor)
        {
            PrintSettings settings = textEditor.Tag as PrintSettings;
            if (settings == null)
            {
                settings = new PrintSettings();
                textEditor.Tag = settings;
            }

            // this baby has all settings to be printed or previewed in the PrintEngine.PrintPreviewDialog
            FlowDocument doc = CreateFlowDocumentForEditor(textEditor);
            
            doc.ColumnWidth = settings.PageSettings.PrintableArea.Width;
            doc.PageHeight = (settings.PageSettings.Landscape ? (int)settings.PrintTicket.PageMediaSize.Width : (int)settings.PrintTicket.PageMediaSize.Height);
            doc.PageWidth = (settings.PageSettings.Landscape ? (int)settings.PrintTicket.PageMediaSize.Height : (int)settings.PrintTicket.PageMediaSize.Width);
            doc.PagePadding = ConvertPageMarginsToThickness(settings.PageSettings.Margins);
            doc.FontFamily = textEditor.FontFamily;
            doc.FontSize = textEditor.FontSize;
            
            return doc;
        }

        /// <summary>
        /// Creates a FlowDocument from TextEditor text.
        /// Add ImageElementGenerator and MathElementGenerator to print images and LaTex mathematical formulas.
        /// by bzquan@gmail.com
        /// </summary>
        static FlowDocument CreateFlowDocumentForEditor(TextEditor editor)
        {
            List<VisualLineElementGenerator> generators = new List<VisualLineElementGenerator>();
            var imageElementGenerator = editor.TextArea.TextView.ElementGenerators.FirstOrDefault(x => x is ImageElementGenerator);
            if (imageElementGenerator != null) generators.Add(imageElementGenerator);

            var mathElementGenerator = editor.TextArea.TextView.ElementGenerators.FirstOrDefault(x => x is MathElementGenerator);
            if (mathElementGenerator != null) generators.Add(mathElementGenerator);

            return ICSharpCode.AvalonEdit.Utils.DocumentPrinter.CreateFlowDocumentForEditor(editor, generators);
        }

        /// <summary>
        /// Converts PaperSize (hundredths of an inch) to PageMediaSize (px).
        /// </summary>
        static PageMediaSize ConvertPaperSizeToMediaSize(PaperSize paperSize)
        {
            return new PageMediaSize(ConvertToPx(paperSize.Width), ConvertToPx(paperSize.Height));
        }

        /// <summary>
        /// Converts specified Margins (hundredths of an inch) to Thickness (px).
        /// </summary>
        static Thickness ConvertPageMarginsToThickness(Margins margins)
        {
            Thickness thickness = new Thickness();
            thickness.Left = ConvertToPx(margins.Left);
            thickness.Top = ConvertToPx(margins.Top);
            thickness.Right = ConvertToPx(margins.Right);
            thickness.Bottom = ConvertToPx(margins.Bottom);
            return thickness;
        }
        
        /// <summary>
        /// Converts specified inch (hundredths of an inch) to pixels (px).
        /// </summary>
        static double ConvertToPx(double inch)
        {
            return inch * 0.96;
        }
    }
}