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
        public int XColumn { get; set; }
        public int YColumn { get; set; }
    }

    public class CurveInfo
    {
        public bool IsGeoCoordinate;
        public string Description;
        public string XColumn;
        public string YColumn;
        public string Option = "";

        public string Title
        {
            get
            {
                if (IsGeoCoordinate)
                    return Description + "(Plane Coornidate)";
                else
                    return Description;
            }
        }
    }

    public class CurveElementGenerator : CustomElementGenerator
    {
        /// mark down syntax: #[Plot description](col1, col2), e.g. #[Plot this is an curve](Lon, Lat)
        private readonly static Regex s_ImageRegex = new Regex(@"#\[Plot(geo)?\s*(bspline|bezier|bcurvature|brcurvature|curvature|tangent|degtangent|simplify_cm|simplify|linreg)?\s*([^\]]*)\]\(([^,]+),(.+)\)", RegexOptions.IgnoreCase);

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
                CurveInfo curveInfo = new CurveInfo();
                curveInfo.IsGeoCoordinate = m.Groups[1].Value.Length > 0;
                curveInfo.Option = m.Groups[2].Value;
                curveInfo.Description = m.Groups[3].Value.Trim();
                curveInfo.XColumn = m.Groups[4].Value.Trim();
                curveInfo.YColumn = m.Groups[5].Value.Trim();

                return CreateUIElement(m.Length, offset, curveInfo);
            }
            return null;
        }

        protected override Regex GetRegex()
        {
            return s_ImageRegex;
        }

        InlineObjectElement CreateUIElement(int length, int offset, CurveInfo curveInfo)
        {
            if (base.IsPrinting)
            {
                BitmapImage bitmap = CurveViewCache.Instance.LoadImage(Document, offset, curveInfo);
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
                OxyPlot.Wpf.PlotView view = CurveViewCache.Instance.LoadPlotView(Document, offset, curveInfo);
                if (view != null)
                    return new InlineObjectElement(length, view);
            }

            return null;
        }
    }
}
