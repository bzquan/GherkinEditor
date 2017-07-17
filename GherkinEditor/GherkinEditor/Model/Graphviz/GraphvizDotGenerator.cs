using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.AvalonEdit.Document;

namespace Gherkin.Model
{
    /// <summary>
    /// http://qiita.com/rubytomato@github/items/51779135bc4b77c8c20d
    /// </summary>
    public class GraphvizDotGenerator
    {
        private int ShapeIndex { get; set; } = -1;
        private int LabelIndex { get; set; } = -1;
        private int ConnIndex { get; set; } = -1;
        private int ColorIndex { get; set; } = -1;
        private string GraphDef { get; set; }
        private string Rankdir { get; set; } = "LR";

        private GraphvizDotModel DotModel { get; set; } = new GraphvizDotModel();

        public static string MakeGraphvizDot(TextDocument document, int offset, string rankdir)
        {
            GraphvizDotGenerator generator = new GraphvizDotGenerator(rankdir);
            var table = GraphvizTableGenerator.MakeTable(document, offset);
            return generator.MakeGraphvizDot(table);
        }

        private GraphvizDotGenerator(string rankdir)
        {
            if (rankdir == "TB" ||  // TB : top to bottom
                rankdir == "BT" ||  // BT : bottom to top
                rankdir == "LR" ||  // LR : left to right
                rankdir == "RL")    // RL : right to left
            {
                Rankdir = rankdir;
            }
        }

        private string MakeGraphvizDot(GraphvizTable table)
        {
            string contents = MakeGraphvizDotContent(table);
            if (string.IsNullOrEmpty(contents)) return null;

            StringBuilder sb = new StringBuilder();
            sb.Append("digraph")
              .Append('{')
              .Append(GetGraphDef())
              .Append(contents)
              .Append('}');

            return sb.ToString();
        }

        private string GetGraphDef()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("graph")
              .Append('[')
              .Append("charset=\"UTF-8\";")
              .Append(GetRankDir())
              .Append("];");

            return sb.ToString();
        }

        private string GetRankDir() => "rankdir=" + Rankdir + ";";

        private string MakeGraphvizDotContent(GraphvizTable table)
        {
            if ((table == null) || (table.RowCount < 2)) return null;

            GraphvizTableRow header = table[0];

            ShapeIndex = header.Index("Shape");
            LabelIndex = header.Index("Label");
            ConnIndex = header.Index("Connections");
            ColorIndex = header.Index("Color");
            for (int i = 1; i < table.RowCount; i++)
            {
                GraphvizTableRow row = table[i];
                bool? isNode = row[0].IsDotNode();
                if (!isNode.HasValue) return null;

                if (isNode.Value)
                {
                    if (row[0].Value.Contains(":"))
                        DotModel.Add(new GraphvizRankNode(row[0].Value));
                    else
                        ProcessNode(row);
                }
                else
                    ProcessEdge(row);
            }

            return DotModel.GetDotString();
        }

        private void ProcessNode(GraphvizTableRow row)
        {
            GraphvizNode node = AddNodeIfNotExist(row[0].Value);
            ProcessNodeAttributes(row, node);
            ProcessNodeConnections(row, node);
        }

        private void ProcessNodeAttributes(GraphvizTableRow row, GraphvizNode node)
        {
            if (LabelIndex >= 0)
            {
                string label = row[LabelIndex]?.Value;
                node.Label = label?.Trim();
            }

            if (ShapeIndex >= 0)
            {
                string shape = row[ShapeIndex]?.Value;
                node.Shape = shape?.Trim();
            }

            if (ColorIndex >= 0)
            {
                string color = row[ColorIndex]?.Value;
                node.FillColor = color?.Trim();
            }
        }

        private void ProcessNodeConnections(GraphvizTableRow row, GraphvizNode node)
        {
            if (ConnIndex <= 0) return;

            string connections = row[ConnIndex].Value;
            if (string.IsNullOrEmpty(connections)) return;

            var nodes = connections.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in nodes)
            {
                string dest = n.Trim();
                if (!string.IsNullOrEmpty(dest))
                {
                    AddEdgeIfNotExist(node.Name, dest, GraphvizEdge.EdgeDir.Normal);
                }
            }
        }

        private void ProcessEdge(GraphvizTableRow row)
        {

            string edges = row[0].Value;
            GraphvizEdge.EdgeDir dir = ToDir(edges);
            var nodes = edges.Split(new string[] { "<->", "->", "--" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                string from = nodes[i].Trim();
                string to = nodes[i + 1].Trim();
                if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                {
                    GraphvizEdge edge = AddEdgeIfNotExist(from, to, dir);
                    ProcessEdgeAttributes(row, edge);
                }
            }
        }

        private GraphvizEdge.EdgeDir ToDir(string edges)
        {
            if (edges.Contains("<->"))
                return GraphvizEdge.EdgeDir.Both;
            else if (edges.Contains("--"))
                return GraphvizEdge.EdgeDir.Non;
            else
                return GraphvizEdge.EdgeDir.Normal;
        }

        private void ProcessEdgeAttributes(GraphvizTableRow row, GraphvizEdge edge)
        {
            if (LabelIndex >= 0)
            {
                string label = row[LabelIndex]?.Value;
                edge.Label = label?.Trim();
            }

            if (ColorIndex >= 0)
            {
                string color = row[ColorIndex]?.Value;
                edge.Color = color?.Trim();
            }

            if (ShapeIndex >= 0)
            {
                string shape = row[ShapeIndex]?.Value;
                edge.ArrowHead = shape?.Trim();
            }
        }

        private GraphvizEdge AddEdgeIfNotExist(string from, string to, GraphvizEdge.EdgeDir dir)
        {
            GraphvizEdge edge = DotModel.FindEdge(from, to);
            if (edge == null)
            {
                edge = new GraphvizEdge(from, to);
                DotModel.Add(edge);
            }
            edge.Dir = dir;
            return edge;
        }

        private GraphvizNode AddNodeIfNotExist(string name)
        {
            GraphvizNode node = DotModel.FindNode(name);
            if (node == null)
            {
                node = new GraphvizNode(name);
                DotModel.Add(node);
            }

            return node;
        }
    }
}
