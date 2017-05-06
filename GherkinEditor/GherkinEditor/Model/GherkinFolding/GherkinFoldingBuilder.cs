using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Document;
using Gherkin.Util;

namespace Gherkin.Model
{
    enum FoldingType { Scenario, Table}
    struct FoldingStarting
    {
        public FoldingType Type { get; set; }
        public int Offset { get; set; }
    }

    class GherkinFoldingBuilder
    {
        private TextDocument Document { get; set; }
        List<NewFolding> newFoldings = new List<NewFolding>();
        private Stack<FoldingStarting> StackedFolderStartingOffsets = new Stack<FoldingStarting>();
        private bool processing_table_row = false;
        private int last_step_tablerow_offset = 0;
        private GherkinSimpleParser parser;

        public GherkinFoldingBuilder(TextDocument document, bool isCloseTablesFolding, bool isCloseScenarioFolding)
        {
            this.Document = document;
            IsCloseTablesFolding = isCloseTablesFolding;
            IsCloseScenarioFolding = isCloseScenarioFolding;
            parser = new GherkinSimpleParser(document);
        }

        private bool IsCloseTablesFolding { get; set; }
        private bool IsCloseScenarioFolding { get; set; }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// Following folding types are supported 
        /// Background, Scenario, ScenarioOutline, Examples, Table
        /// </summary>
        public List<NewFolding> BuildFoldings()
        {
            DocumentLine line = Document.GetLineByNumber(1);
            while (line != null)
            {
                TokenType line_type = parser.Parse(line);
                switch (line_type)
                {
                    case TokenType.BackgroundLine:
                    case TokenType.ScenarioLine:
                    case TokenType.ScenarioOutlineLine:
                        BuildStackedFoldings();
                        processing_table_row = false;
                        StartScenarioFolding(line);
                        last_step_tablerow_offset = line.Offset + line.Length;
                        break;
                    case TokenType.StepLine:
                        BuildTableFolding();
                        last_step_tablerow_offset = line.Offset + line.Length;
                        break;
                    case TokenType.TableRow:
                        StartTableFolding(line);
                        last_step_tablerow_offset = line.Offset + line.Length;
                        break;
                    case TokenType.ExamplesLine:
                        BuildTableFolding();
                        break;
                    default:
                        break;
                }
                line = line.NextLine;
            }

            BuildStackedFoldings();
            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }

        private void BuildStackedFoldings()
        {
            while (StackedFolderStartingOffsets.Count > 0)
            {
                BuildFolder();
            }
        }

        private void BuildTableFolding()
        {
            if (processing_table_row)
            {
                BuildFolder();
                processing_table_row = false;
            }
        }

        private void BuildFolder()
        {
            if (StackedFolderStartingOffsets.Count == 0) return;

            FoldingStarting startOffset = StackedFolderStartingOffsets.Pop();
            if (startOffset.Offset < last_step_tablerow_offset)
            {
                NewFolding folding = new NewFolding(startOffset.Offset, last_step_tablerow_offset);
                if (startOffset.Type == FoldingType.Scenario)
                {
                    folding.DefaultClosed = IsCloseScenarioFolding;
                    folding.IsDefinition = true;
                }
                else
                {
                    folding.DefaultClosed = IsCloseTablesFolding;
                }

                DocumentLine start_line = Document.GetLineByOffset(startOffset.Offset);
                folding.Name = GherkinFormatUtil.GetText(Document, start_line);
                newFoldings.Add(folding);
            }
        }

        private void StartScenarioFolding(DocumentLine line)
        {
            FoldingStarting start = new FoldingStarting() { Type = FoldingType.Scenario, Offset = line.Offset };
            StackedFolderStartingOffsets.Push(start);
        }

        private void StartTableFolding(DocumentLine line)
        {
            if (!processing_table_row)
            {
                FoldingStarting start = new FoldingStarting() { Type = FoldingType.Table, Offset = line.Offset };
                StackedFolderStartingOffsets.Push(start);
                processing_table_row = true;
            }
        }
    }
}
