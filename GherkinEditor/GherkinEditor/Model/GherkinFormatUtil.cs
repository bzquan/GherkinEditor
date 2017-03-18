using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gherkin.Model
{
    public static class GherkinFormatUtil
    {
        public static string GetText(TextDocument document, DocumentLine line)
        {
            return document.GetText(line.Offset, line.TotalLength);
        }

        public static string MakeTwoLines(string first, string second)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(first).Append(second);

            return sb.ToString();
        }

        public static string MakeThreeLines(string first, string second, string third)
        {
            StringBuilder sb = new StringBuilder();
            if (first.Length > 0)
                sb.AppendLine(first);
            sb.AppendLine(second)
              .Append(third);

            return sb.ToString();
        }

        public static bool IsTableRow(string text)
        {
            string trimmed = text.Trim();
            if (trimmed.Length < 2) return false;
            if (!trimmed.StartsWith("|", StringComparison.Ordinal)) return false;
            return trimmed.EndsWith("|", StringComparison.Ordinal);
        }

        public static bool MakeFormattedTable(TextDocument document, DocumentLine line)
        {
            GherkinTableBuilder builder = new GherkinTableBuilder(document, line);
            GherkinTable table = builder.Build();
            if (table == null) return false;

            string table_text = table.Format();
            document.Replace(builder.Offset, builder.Length, table_text);

            return true;
        }

        public static Tuple<DocumentLine, string> FormatTable(TextDocument document, DocumentLine line, int endLine)
        {
            Tuple<DocumentLine, List<string>> table_rows = ExtractTableText(document, line, endLine);
            GherkinTable table = GherkinTableBuilder.BuildTable(table_rows.Item2);
            string table_text = table.Format();

            return new Tuple<DocumentLine, string>(table_rows.Item1, table_text);
        }

        private static Tuple<DocumentLine, List<string>> ExtractTableText(TextDocument document, DocumentLine line, int endLine)
        {
            List<string> table_rows = new List<string>();
            while ((line != null) && (line.LineNumber <= endLine))
            {
                string line_text = GetText(document, line);
                if (IsTableRow(line_text))
                {
                    table_rows.Add(line_text);
                }
                else
                {
                    break;
                }
                line = line.NextLine;
            }

            return new Tuple<DocumentLine, List<string>>(line, table_rows);
        }

        public static bool IsTag(string line)
        {
            return line.TrimStart().StartsWith("@", StringComparison.Ordinal);
        }

        public static bool IsEmptyOrCommentLine(string line)
        {
            string trimmed = line.TrimStart();
            return (trimmed.Length == 0) || trimmed.StartsWith("#", StringComparison.Ordinal);
        }

        public static string MakeGUID(TextDocument document, DocumentLine line)
        {
            while (line != null)
            {
                string text = GetText(document, line);
                if (!(IsTag(text) || IsEmptyOrCommentLine(text))) break;    // Not GUID
                if (text.Trim().StartsWith("@guid-", StringComparison.Ordinal))
                {
                    // GUID exists already
                    return "";
                }

                line = line.PreviousLine;
            }

            return "@guid-" + Guid.NewGuid();
        }
    }
}
