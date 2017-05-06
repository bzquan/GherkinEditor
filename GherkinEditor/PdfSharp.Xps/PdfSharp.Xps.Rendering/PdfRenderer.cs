using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Xps.XpsModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;

namespace PdfSharp.Xps.Rendering
{
    /// <summary>
    /// Implements the rendering process.
    /// </summary>
    class PdfRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRenderer"/> class.
        /// </summary>
        internal PdfRenderer()
        {
            //this.document = new PdfDocument();
            //this.page = this.document.AddPage();
            //this.context = context;
        }

        internal PdfPage CreatePage(PdfDocument doc, FixedPage fixedPage)
        {
            PdfPage page = doc.Pages.Add();
            page.Width = XUnit.FromPresentation(fixedPage.Width);
            page.Height = XUnit.FromPresentation(fixedPage.Height);
            return page;
        }

        internal void RenderPage(PdfPage page, FixedPage fixedPage)
        {
            this.page = page;

            this.context = new DocumentRenderingContext(page.Owner);

            PdfContent content = page.Contents.AppendContent();
            page.RenderContent = content;

            this.writer = new PdfContentWriter(this.context, this.page);
            this.writer.BeginContent(false);
            this.writer.WriteElements(fixedPage.Content);
            this.writer.EndContent();
        }

        internal PdfDocument Document
        {
            get { return this.page.document; }
        }

        PdfPage page;
        PdfContentWriter writer;
        DocumentRenderingContext context;
    }
}