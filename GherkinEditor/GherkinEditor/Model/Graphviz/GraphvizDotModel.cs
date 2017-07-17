using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gherkin.Model
{
    public class GraphvizElement
    {
        public string Label { get; set; }

        public virtual string GetDotString()
        {
            if (string.IsNullOrEmpty(Label)) return null;

            StringBuilder sb = new StringBuilder();
            sb.Append("label=")
              .Append("\"")
              .Append(Label)
              .Append("\",")
              .Append(GetFontName(Label));

            return sb.ToString();
        }

        protected string GetFontName(string text)
        {
            if (Util.StringUtil.ContainsUnicodeCharacter(text))
            {
                return $"fontname =\"{GraphvizCache.Instance.FontFamilyName4GraphvizUnicode}\",";
            }
            else
                return null;
        }
    }

    public class GraphvizEdge : GraphvizElement
    {
        public enum EdgeDir { Normal, Both, Non}
        public GraphvizEdge(string from, string to)
        {
            From = from;
            To = to;
        }

        public string From { get; private set; }
        public string To { get; private set; }
        public string Color { get; set; }
        public EdgeDir Dir { get; set; }
        public string ArrowHead { get; set; }

        public override string GetDotString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(From)
              .Append("->")
              .Append(To)
              .Append(GetAttributes())
              .Append(";");

            return sb.ToString();
        }

        private string GetAttributes()
        {
            StringBuilder attr = new StringBuilder();
            attr.Append(base.GetDotString());
            attr.Append(GetColorDotString());
            attr.Append(GetEdgeDirection());
            attr.Append(GetArrowHead());

            if (attr.Length > 0)
                return "[" + attr + "]";
            else
                return null;
        }

        private string GetColorDotString()
        {
            if (string.IsNullOrEmpty(Color)) return null;
            return "color=" + Color + ", fontcolor=" + Color + ",";
        }

        private string GetEdgeDirection()
        {
            switch (Dir)
            {
                case EdgeDir.Both:
                    return "dir=both,";
                case EdgeDir.Non:
                    return "arrowhead=none,";
                default:
                    return null;
            }
        }

        private string GetArrowHead()
        {
            if (string.IsNullOrEmpty(ArrowHead)) return null;
            return "arrowhead=" + ArrowHead + ",";
        }
    }

    /// <summary>
    /// Graphbiz ndoe
    /// Example of supported shapes:
    /// box	箱形
    /// ellipse (default) 楕円
    /// oval	卵型
    /// circle	円
    /// egg	卵
    /// triangle	三角形
    /// plaintext	テキスト
    /// plain	テキスト
    /// diamond	ダイヤモンド
    /// trapezium	台形
    /// tab	タブ
    /// folder	フォルダー
    /// </summary>
    public class GraphvizNode : GraphvizElement
    {
        public GraphvizNode(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
        public string FillColor { get; set; }
        public string Shape { get; set; }

        public override string GetDotString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name)
              .Append(GetAttributes())
              .Append(";");

            return sb.ToString();
        }

        private string GetAttributes()
        {
            StringBuilder attr = new StringBuilder();
            attr.Append(base.GetDotString())
                .Append(GetFillColorDotString())
                .Append(GetShapeDotString())
                .Append(GetFontName(Name));

            if (attr.Length > 0)
                return "[" + attr + "]";
            else
                return null;
        }

        private string GetFillColorDotString()
        {
            if (string.IsNullOrEmpty(FillColor)) return null;
            return "fillcolor=" + FillColor + ", style=filled,";
        }

        private string GetShapeDotString()
        {
            if (string.IsNullOrEmpty(Shape)) return null;
            return "shape=" + Shape + ",";
        }
    }

    public class GraphvizRankNode : GraphvizElement
    {
        public GraphvizRankNode(string nodelist)
        {
            NodeList = nodelist;
        }

        public string NodeList { get; private set; }

        public override string GetDotString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
              .Append("rank = same;");
            var nodes = NodeList.Split(':');
            foreach (var node in nodes)
            {
                string n = node.Trim();
                if (!string.IsNullOrEmpty(n))
                {
                    sb.Append(n)
                      .Append(";");
                }
            }
            sb.Append("}");

            return sb.ToString();
        }
    }


    public class GraphvizDotModel
    {
        private List<GraphvizNode> Nodes { get; set; } = new List<GraphvizNode>();
        private List<GraphvizEdge> Edges { get; set; } = new List<GraphvizEdge>();
        private List<GraphvizRankNode> RankNodes { get; set; } = new List<GraphvizRankNode>();

        public void Add(GraphvizNode node) => Nodes.Add(node);

        public GraphvizNode FindNode(string name) => Nodes.FirstOrDefault(x => x.Name == name);

        public void Add(GraphvizEdge edge) => Edges.Add(edge);

        public GraphvizEdge FindEdge(string fromName, string toName)
        {
            return Edges.FirstOrDefault(x => x.From == fromName && x.To == toName);
        }
        public void Add(GraphvizRankNode node) => RankNodes.Add(node);

        public string GetDotString()
        {
            StringBuilder sb = new StringBuilder();
            Nodes.ForEach(x => sb.Append(x.GetDotString()));
            Edges.ForEach(x => sb.Append(x.GetDotString()));
            RankNodes.ForEach(x => sb.Append(x.GetDotString()));

            return sb.ToString();
        }
    }
}
