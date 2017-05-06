using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using static Gherkin.Model.GherkinFormatUtil;
using Gherkin.Util;

namespace Gherkin.Model
{
    public class GherkinSimpleParser
    {
        public const string IDENT2 = "  ";     // 2 spaces
        public const string IDENT4 = "    ";   // 4 spaces

        TokenMatcher TokenMatcher { get; set; } = new TokenMatcher();
        TextDocument m_Doc;

        public GherkinSimpleParser(TextDocument document)
        {
            m_Doc = document;
            TokenMatcher.CurrentDialect = this.CurrentDialect;
        }

        public Tuple<TokenType, string> Format(string line)
        {
            string formatted_line;
            TokenType keyword;
            if (IsKeyWord(line, out formatted_line, out keyword))
            {
                return new Tuple<TokenType, string>(keyword, formatted_line);
            }
            else if (IsStep(line, out formatted_line))
                return new Tuple<TokenType, string>(TokenType.StepLine, formatted_line);
            else if (IsTag(line, out formatted_line))
                return new Tuple<TokenType, string>(TokenType.TagLine, formatted_line);
            else if (IsComment(line, out formatted_line))
                return new Tuple<TokenType, string>(TokenType.Comment, formatted_line);
            else if (IsTableRow(line))
                return new Tuple<TokenType, string>(TokenType.TableRow, "");

            return new Tuple<TokenType, string>(TokenType.Other, line.TrimEnd());
        }

        public string Format(int beginLine, int endLine)
        {
            StringBuilder sb = new StringBuilder();
            DocumentLine line = m_Doc.GetLineByNumber(beginLine);
            while ((line != null) && (line.LineNumber <= endLine))
            {
                string line_text = GetText(m_Doc, line);
                TryUpdateLanguage(line_text);
                Tuple<TokenType, string> result = Format(line_text);
                switch (result.Item1)
                {
                    case TokenType.ScenarioLine:
                    case TokenType.ScenarioOutlineLine:
                        string guid_tag = MakeGUID(m_Doc, line.PreviousLine);
                        if (guid_tag.Length > 0)
                            sb.AppendLine(guid_tag);
                        sb.AppendLine(result.Item2);
                        line = line.NextLine;
                        break;
                    case TokenType.TableRow:
                        Tuple<DocumentLine, string> table_result = FormatTable(m_Doc, line, endLine);
                        sb.Append(table_result.Item2);
                        line = table_result.Item1;
                        break;
                    default:
                        sb.AppendLine(result.Item2);
                        line = line.NextLine;
                        break;
                }
            }

            return sb.ToString();
        }

        private Token ToToken(string line)
        {
            var location = new Ast.Location(1);
            return new Token(new GherkinLine(line, 1), location);
        }

        public Token ParseToken(string line)
        {
            Token token = ToToken(line);
            if (TokenMatcher.Match_FeatureLine(token) ||
                TokenMatcher.Match_ScenarioLine(token) ||
                TokenMatcher.Match_ScenarioOutlineLine(token) ||
                TokenMatcher.Match_BackgroundLine(token) ||
                TokenMatcher.Match_ExamplesLine(token) ||
                TokenMatcher.Match_StepLine(token) ||
                TokenMatcher.Match_TagLine(token) ||
                TokenMatcher.Match_TableRow(token) ||
                SimpleMatchLanguage(token) ||
                TokenMatcher.Match_DocStringSeparator(token))
            {
                return token;
            }

            return token;
        }

        /// <summary>
        /// Parse first 6 lines to match language tag and update current Dialect
        /// </summary>
        public GherkinDialect CurrentDialect
        {
            get
            {
                const int MAX_TRY_COUNT = 6;
                int tryCount = 0;
                var lines = m_Doc.Lines;
                foreach (var line in m_Doc.Lines)
                {
                    string text = GherkinFormatUtil.GetText(m_Doc, line);
                    tryCount++;
                    Token token = ToToken(text);
                    if (SimpleMatchLanguage(token) || tryCount >= MAX_TRY_COUNT) break;
                }

                return TokenMatcher.CurrentDialect;
            }
        }

        private bool SimpleMatchLanguage(Token token)
        {
            try
            {
                if (TokenMatcher.Match_Language(token))
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public TokenType Parse(DocumentLine line)
        {
            string line_text = GetText(m_Doc, line);
            TokenType line_type;

            TryUpdateLanguage(line_text);

            if (IsKeyWord(line_text, out line_type)) return line_type;
            if (IsStep(line_text)) return TokenType.StepLine;
            if (IsTableRow(line_text)) return TokenType.TableRow;

            return TokenType.Other;
        }

        private void TryUpdateLanguage(string line)
        {
            Token token = ToToken(line);
            try
            {
                if (TokenMatcher.Match_Language(token))
                {
                    NotifyCurrentGherkinLanguage(token.MatchedText);
                }
            }
            catch (Exception ex)
            {
                EventAggregator<StatusChangedArg>.Instance.Publish(this, new StatusChangedArg(ex.Message));
            }
        }

        private void NotifyCurrentGherkinLanguage(string language)
        {
            EventAggregator<CurrentGherkinLanguageArg>.Instance.Publish(this, new CurrentGherkinLanguageArg(language));
        }

        public void AppendMissingScenarioGUID()
        {
            if (!GherkinFormatUtil.AppSettings.GenerateGUIDforScenario) return;

            DocumentLine line = m_Doc.GetLineByNumber(1);
            while (line != null)
            {
                string line_text = GetText(m_Doc, line);
                TryUpdateLanguage(line_text);
                if (IsScenarioKeyWord(line_text))
                {
                    string tag = MakeGUID(m_Doc, line.PreviousLine);
                    if (tag.Length > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(tag).Append(line_text);
                        m_Doc.Replace(line.Offset, line.TotalLength, sb.ToString());
                    }
                }

                line = line.NextLine;
            }
        }

        private bool IsScenarioKeyWord(string line_text)
        {
            TokenType keywordType;
            if (!IsKeyWord(line_text, out keywordType)) return false;
            return (keywordType == TokenType.ScenarioLine) || (keywordType == TokenType.ScenarioOutlineLine);
        }

        private bool IsKeyWord(string line, out string formatted_line, out TokenType keywordType)
        {
            formatted_line = "";
            keywordType = TokenType.Other;
            Token token = ToToken(line);
            if (TokenMatcher.Match_FeatureLine(token) ||
                TokenMatcher.Match_ScenarioLine(token) ||
                TokenMatcher.Match_ScenarioOutlineLine(token) ||
                TokenMatcher.Match_BackgroundLine(token))
            {
                string keyword = token.MatchedKeyword;
                keywordType = token.MatchedType;
                formatted_line = keyword + ": " + token.MatchedText;
                return true;
            }
            if (TokenMatcher.Match_ExamplesLine(token))
            {
                string keyword = token.MatchedKeyword;
                keywordType = token.MatchedType;
                formatted_line = IDENT2 + keyword + ": " + token.MatchedText;
                return true;
            }

            return false;
        }

        private bool IsKeyWord(string line, out TokenType keywordType)
        {
            keywordType = TokenType.Other;
            Token token = ToToken(line);
            if (TokenMatcher.Match_FeatureLine(token) ||
                TokenMatcher.Match_ScenarioLine(token) ||
                TokenMatcher.Match_ScenarioOutlineLine(token) ||
                TokenMatcher.Match_BackgroundLine(token) ||
                TokenMatcher.Match_ExamplesLine(token))
            {
                string keyword = token.MatchedKeyword;
                keywordType = token.MatchedType;

                if (keywordType == TokenType.FeatureLine)
                {
                    NotifyCurrentGherkinLanguage(token.MatchedGherkinDialect.Language);
                }

                return true;
            }

            return false;
        }

        private bool IsStep(string line, out string formatted_line)
        {
            formatted_line = "";
            Token token = ToToken(line);
            if (TokenMatcher.Match_StepLine(token))
            {
                string keyword = token.MatchedKeyword;
                formatted_line = IDENT2 + keyword.Trim() + " " + token.MatchedText;
                return true;
            }

            return false;
        }

        private bool IsStep(string line)
        {
            Token token = ToToken(line);
            return TokenMatcher.Match_StepLine(token);
        }

        private bool IsTag(string line, out string formatted_line)
        {
            Token token = ToToken(line);
            formatted_line = line.TrimEnd();
            return TokenMatcher.Match_TagLine(token);
        }

        private bool IsComment(string line, out string formatted_line)
        {
            Token token = ToToken(line);
            formatted_line = line.TrimEnd();
            return TokenMatcher.Match_Comment(token);
        }
    }
}
