﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Document;
using static Gherkin.Model.GherkinFormatUtil;
using static Gherkin.Util.StringBuilderExtension;
using Gherkin.Util;
using ICSharpCode.AvalonEdit;

namespace Gherkin.Model
{
    public class GherkinIndentationStrategy : DefaultIndentationStrategy
    {
        private TextEditor Editor { get; set; }

        public GherkinIndentationStrategy(TextEditor editor)
        {
            Editor = editor;
        }

        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            if (line == null) throw new ArgumentNullException(nameof(line));

            DocumentLine previousLine = line.PreviousLine;
            if (previousLine == null) return;

            GherkinSimpleParser parser = new GherkinSimpleParser(document);
            Tuple<TokenType, string> result = parser.Format(GetText(document, previousLine));
            switch (result.Item1)
            {
                case TokenType.FeatureLine:
                case TokenType.BackgroundLine:
                case TokenType.ExamplesLine:
                case TokenType.StepLine:
                    document.Replace(previousLine.Offset, previousLine.TotalLength,
                                     MakeTwoLines(result.Item2, GherkinSimpleParser.IDENT2));
                    break;
                case TokenType.ScenarioLine:
                case TokenType.ScenarioOutlineLine:
                    string guid_tag = MakeGUID(document, previousLine.PreviousLine);
                    document.Replace(previousLine.Offset, previousLine.TotalLength,
                                     MakeThreeLines(guid_tag, result.Item2, GherkinSimpleParser.IDENT2));
                    break;
                case TokenType.TableRow:
                    int lineNo = line.LineNumber;
                    if (MakeFormattedTable(document, line))
                        MoveCursorToFirstCellOfTableRow(lineNo);
                    else
                        base.IndentLine(document, line);
                    break;
                default:
                    base.IndentLine(document, line);
                    break;
            }
        }

        public override void IndentLines(TextDocument document, int beginLine, int endLine)
        {
            DocumentLine line = document.GetLineByNumber(beginLine);
            int offset = line?.Offset ?? -1;
            int length = CalcSegmentLength(line, endLine);
            if ((offset == -1) || (length == 0)) return;

            IndentationCompletedArg eventArg = PrepareIndentationCompletedArg(document);

            GherkinSimpleParser parser = new GherkinSimpleParser(document);
            string formatted_text = parser.Format(beginLine, endLine);
            if (endLine == document.LineCount)
            {
                StringBuilder sb = new StringBuilder(formatted_text);
                sb.TrimEnd().AppendLine();
                formatted_text = sb.ToString();
            }
            document.Replace(offset, length, formatted_text);

            EventAggregator<IndentationCompletedArg>.Instance.Publish(document, eventArg);
        }

        private IndentationCompletedArg PrepareIndentationCompletedArg(TextDocument document)
        {
            int currentLineNo = Editor.TextArea.Caret.Line;
            int currentColumnNo = Editor.TextArea.Caret.Column;
            DocumentLine line = document.GetLineByNumber(currentLineNo);
            string line_text = GetText(document, line);

            return new IndentationCompletedArg(currentLineNo, currentColumnNo, line_text);
        }

        private int CalcSegmentLength(DocumentLine beginLine, int endLine)
        {
            DocumentLine line = beginLine;
            int length = 0;
            while ((line != null) && (line.LineNumber <= endLine))
            {
                length += line.TotalLength;
                line = line.NextLine;
            }
            return length;
        }

        private void MoveCursorToFirstCellOfTableRow(int lineNo)
        {
            TextDocument document = Editor.Document;
            DocumentLine line = document.GetLineByNumber(lineNo);

            string line_text = GetText(document, line);
            int column = line_text.IndexOf('|');
            if (column >= 0)
            {
                int offset = line.Offset + column + 1;
                Editor.TextArea.Caret.Offset = offset;
            }
        }
    }
}
