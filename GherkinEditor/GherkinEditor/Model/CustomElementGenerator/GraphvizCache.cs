using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Gherkin.Util;

namespace Gherkin.Model
{
    public class GraphvizCache : CacheBase
    {
        private static readonly Lazy<GraphvizCache> s_Singleton =
            new Lazy<GraphvizCache>(() => new GraphvizCache());

        private List<GraphvizImage> m_GraphbizImages = new List<GraphvizImage>();

        public static GraphvizCache Instance => s_Singleton.Value;

        private GraphvizCache()
        {
            var setting = new AppSettings();
            FontFamilyName4GraphvizUnicode = setting.Fonts.FontFamilyName4GraphvizUnicode;

            EventAggregator<GraphvizFontnameChangedArg>.Instance.Event += OnFontnameChanged;
        }

        public string FontFamilyName4GraphvizUnicode { get; set; }

        private void OnFontnameChanged(object sender, GraphvizFontnameChangedArg arg)
        {
            FontFamilyName4GraphvizUnicode = arg.Fontname4Unicode;
            m_GraphbizImages.Clear();
        }

        public BitmapImage LoadImage(string dotGraph, GraphGeneration graphGeneration)
        {
            string key = MakeKey(dotGraph);
            GraphvizImage graphviz = m_GraphbizImages.FirstOrDefault(x => x.Key == key);
            if (graphviz != null)
                m_GraphbizImages.Remove(graphviz);
            else
                graphviz = new GraphvizImage(dotGraph, key);

            m_GraphbizImages.Insert(0, graphviz);
            Util.Util.RemoveLastItems(m_GraphbizImages, max_num: CacheSize);
            return graphviz.LoadImage(graphGeneration);
        }

        private static string MakeKey(string dotGraph)
        {
            string key = string.Format("{0}-{1}", dotGraph.Length, dotGraph.GetHashCode());
            return key;
        }

        class GraphvizImage
        {
            private BitmapImage m_BitmapImage;
            private string m_dotGraph;
            private bool HasLoaded { get; set; } = false;

            public GraphvizImage(string dotGraph, string key)
            {
                m_dotGraph = dotGraph;
                Key = key;
            }

            public string Key { get; private set; }

            public BitmapImage LoadImage(GraphGeneration graphGeneration)
            {
                if (!HasLoaded)
                {
                    m_BitmapImage = MakeBitmapImage(graphGeneration);
                    HasLoaded = true;
                }

                return m_BitmapImage;
            }

            private BitmapImage MakeBitmapImage(GraphGeneration graphGeneration)
            {
                try
                {
                    byte[] output;
                    if (Util.StringUtil.ContainsUnicodeCharacter(m_dotGraph))
                        output = graphGeneration.GenerateGraphViaFile(m_dotGraph, Enums.GraphReturnType.Png);
                    else
                        output = graphGeneration.GenerateGraphDirectly(m_dotGraph, Enums.GraphReturnType.Png);

                    return Util.Util.BitmapImageFromImage(output);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
