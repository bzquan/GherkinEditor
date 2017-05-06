using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using Gherkin.Util;
using WpfMath;
using System.Windows;
using System.Windows.Input;

namespace Gherkin.Model
{
    /// <summary>
    /// Math Element Generators for AvalonEdit
    /// 
    /// To use this class:
    /// textEditor.TextArea.TextView.ElementGenerators.Add(new MathElementGenerator());
    /// 
    /// Image must be specified by following mark down.
    /// mark down syntax: ![LaTex scale](LaTeX typsetting), e.g. ![LaTex 20](\sqrt{\frac{ax^2+bx+\pi}{x^3}})
    /// The scale is a value between 10 to 100. It is optional and the default value is 20. 
    /// 
    /// http://danielgrunwald.de/coding/AvalonEdit/rendering.php
    /// https://github.com/ForNeVeR/wpf-math
    /// </summary>
    public class MathElementGenerator : CustomElementGenerator
    {
        public static readonly string LaTexPrefix = "![LaTex";
        // mark down syntax: ![LaTex scale](LaTeX typsetting), e.g. ![LaTex 20](\sqrt{\frac{ax^2+bx+\pi}{x^3}})
        private readonly static Regex s_MathRegex = new Regex(@"!\[LaTex\s*([0-9]*)\]\((.+)\)", RegexOptions.IgnoreCase);

        public MathElementGenerator(TextDocument document) : base(document)
        {
        }

        /// Constructs an element at the specified offset.
        /// May return null if no element should be constructed.
        protected override InlineObjectElement ConstructMainElement(int offset)
        {
            Match m = FindMatch(offset);
            // check whether there's a match exactly at offset
            if (m.Success && m.Index == 0)
            {
                string scale = m.Groups[1].Value;
                string laTex = m.Groups[2].Value;
                BitmapImage bitmap = TaTexImageCache.Instance.LoadImage(laTex, scale);
                if (bitmap != null)
                {
                    Image image = new Image();
                    image.Source = bitmap;

                    image.Width = bitmap.PixelWidth;
                    image.Height = bitmap.PixelHeight;
                    image.Cursor = Cursors.Arrow;
                    // Pass the length of the match to the 'documentLength' parameter
                    // of InlineObjectElement.
                    return new InlineObjectElement(m.Length, image);
                }
            }
            return null;
        }

        protected override Regex GetRegex()
        {
            return s_MathRegex;
        }
    }
}
