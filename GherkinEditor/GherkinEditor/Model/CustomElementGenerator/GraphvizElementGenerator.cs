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
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using ICSharpCode.AvalonEdit;

using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;

namespace Gherkin.Model
{
    /// <summary>
    /// Image Element Generators for AvalonEdit
    /// 
    /// To use this class:
    /// textEditor.TextArea.TextView.ElementGenerators.Add(new GraphvizElementGenerator());
    /// 
    /// Image must be specified by following mark down.
    /// mark down syntax: #[Graphviz scale description](image path), e.g. #[Graphviz 80 this is an image](Gherkin.png)
    /// Description is optional.
    /// BasePath is the directory of document.FileName
    /// http://danielgrunwald.de/coding/AvalonEdit/rendering.php
    /// </summary>
    public class GraphvizElementGenerator : CustomElementGenerator
    {
        public static readonly string ImagePrefix = "#[Graphviz";
        /// mark down syntax: #[Graphviz scale rankdir], e.g. #[Graphviz], #[Graphviz 80], #[Graphviz 80 TB]
        /// rankdir -> TB : top to bottom
        ///            BT : bottom to top
        ///            LR : left to right
        ///            RL : right to left
        private readonly static Regex s_ImageRegex = new Regex(@"#\[Graphviz\s*([0-9]*)([^\]]*)\]", RegexOptions.IgnoreCase);

        private static GraphGeneration GraphGenerationSingleton { get; set; }

        public GraphvizElementGenerator(TextEditor textEditor) : base(textEditor)
        {
        }

        protected override Regex GetRegex()
        {
            return s_ImageRegex;
        }

        /// Constructs an element at the specified offset.
        /// May return null if no element should be constructed.
        protected override InlineObjectElement ConstructMainElement(int offset)
        {
            Match m = FindMatch(offset);
            // check whether there's a match exactly at offset
            if (m.Success && m.Index == 0)
            {
                GraphGeneration wrapper = GetGraphGeneration();
                UIElement uiElement;
                if (wrapper.IsGraphvizInstalled)
                {
                    string rankdir = m.Groups[2].Value.Trim();
                    string dotGraph = GraphvizDotGenerator.MakeGraphvizDot(Document, offset, rankdir);
                    BitmapImage bitmap = LoadBitmap(dotGraph);
                    if (bitmap != null)
                    {
                        string scale = m.Groups[1].Value;
                        uiElement = CreateImageControl(offset, scale, bitmap);
                    }
                    else
                    {
                        uiElement = CreateErrorMesageTextBlock("Invalid Graphviz");
                    }
                }
                else
                {
                    uiElement = CreateErrorMesageTextBlock("Graphviz is not installed");
                }

                // Pass the length of the match to the 'documentLength' parameter of InlineObjectElement.
                return new InlineObjectElement(m.Length, uiElement);
            }

            return null;
        }

        private GraphGeneration GetGraphGeneration()
        {
            if (GraphGenerationSingleton == null)
            {
                // These three instances can be injected via the IGetStartProcessQuery, 
                //                                               IGetProcessStartInfoQuery and 
                //                                               IRegisterLayoutPluginCommand interfaces
                var getStartProcessQuery = new GetStartProcessQuery();
                var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
                var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

                GraphGenerationSingleton = new GraphGeneration(getStartProcessQuery,
                                                      getProcessStartInfoQuery,
                                                      registerLayoutPluginCommand);
            }

            return GraphGenerationSingleton;
        }

        private BitmapImage LoadBitmap(string dotGraph)
        {
            if (!string.IsNullOrEmpty(dotGraph))
                return GraphvizCache.Instance.LoadImage(dotGraph, GetGraphGeneration());
            else
                return null;
        }

        private Image CreateImageControl(int offset, string scale, BitmapImage bitmap)
        {
            CustomImageControl image = new CustomImageControl(offset, TextEditor);
            image.Source = bitmap;

            double zoom = CalcZoom(bitmap, scale);
            image.Width = bitmap.PixelWidth * zoom;
            image.Height = bitmap.PixelHeight * zoom;
            image.Cursor = Cursors.Arrow;

            return image;
        }

        private static double CalcZoom(BitmapImage bitmap, string scale)
        {
            double scale_value = ImageScale(scale);
            double width = Math.Min(1024, bitmap.PixelWidth * scale_value);
            double height = Math.Min(1024, bitmap.PixelHeight * scale_value);
            double zoomX = width / (double)bitmap.PixelWidth;
            double zoomY = height / (double)bitmap.PixelHeight;

            return Math.Min(zoomX, zoomY);
        }

        /// <summary>
        /// Scale would be between 0.1 and 5.0
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static double ImageScale(string scale)
        {
            double scale_value = 1.0;
            if (!string.IsNullOrWhiteSpace(scale))
            {
                scale_value = int.Parse(scale) / 100.0;
            }

            scale_value = Math.Max(0.1, scale_value);
            scale_value = Math.Min(5.0, scale_value);

            return scale_value;
        }

        private TextBlock CreateErrorMesageTextBlock(string errorMsg)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = errorMsg,
                Foreground = new SolidColorBrush(Colors.Red),
                Cursor = Cursors.Arrow
            };

            return textBlock;
        }
    }
}
