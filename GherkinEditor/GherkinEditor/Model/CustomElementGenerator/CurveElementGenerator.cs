using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Gherkin.Model
{
    class TableHeader
    {
        public DocumentLine HeaderLine { get; set; }

        public string Description { get; set; }

        public string XTitle { get; set; }
        public int XColumn { get; set; }

        public string YTitle { get; set; }
        public int YColumn { get; set; }
    }

    public class CurveElementGenerator : CustomElementGenerator
    {
        /// mark down syntax: #[Plot description](col1, col2), e.g. #[Plot this is an curve](Lon, Lat)
        private readonly static Regex s_ImageRegex = new Regex(@"#\[Plot\s*(spline)?\s*([^\]]*)\]\(([^,]+),(.+)\)", RegexOptions.IgnoreCase);

        public CurveElementGenerator(TextEditor textEditor) : base(textEditor)
        {
        }

        protected override bool CanApplyGenerator()
        {
            return GherkinUtil.IsFeatureFile(Document.FileName);
        }

        protected override InlineObjectElement ConstructMainElement(int offset)
        {
            Match m = FindMatch(offset);
            // check whether there's a match exactly at offset
            if (m.Success && m.Index == 0)
            {
                string spline = m.Groups[1].Value.Trim();
                string description = m.Groups[2].Value.Trim();
                string xColumn = m.Groups[3].Value.Trim();
                string yColumn = m.Groups[4].Value.Trim();

                return CreateUIElement(m.Length, offset, description, xColumn, yColumn, spline);
            }
            return null;
        }

        protected override Regex GetRegex()
        {
            return s_ImageRegex;
        }

        InlineObjectElement CreateUIElement(int length, int offset, string description, string xColumn, string yColumn, string spline)
        {
            if (base.IsPrinting)
            {
                BitmapImage bitmap = CurveViewCache.Instance.LoadImage(Document, offset, description, xColumn, yColumn, spline);
                if (bitmap != null)
                {
                    var image = new System.Windows.Controls.Image();
                    image.Source = bitmap;

                    image.Width = bitmap.PixelWidth;
                    image.Height = bitmap.PixelHeight;
                    image.Cursor = Cursors.Arrow;
                    // Pass the length of the match to the 'documentLength' parameter
                    // of InlineObjectElement.
                    return new InlineObjectElement(length, image);
                }
            }
            else
            {
                OxyPlot.Wpf.PlotView view = CurveViewCache.Instance.LoadPlotView(Document, offset, description, xColumn, yColumn, spline);
                if (view != null)
                    return new InlineObjectElement(length, view);
            }

            return null;
        }
    }
}
