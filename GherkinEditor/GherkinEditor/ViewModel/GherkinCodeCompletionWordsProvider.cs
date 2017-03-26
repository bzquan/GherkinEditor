﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;
using Gherkin.Model;
using Gherkin.Util;

namespace Gherkin.ViewModel
{
    public class EnteredText
    {
        public EnteredText(string text, int line, int column)
        {
            Text = text;
            Line = line;
            Column = column;
        }

        public string Text { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
    }

    public class GherkinCodeCompletionWordsProvider
    {
        private TextDocument Document { get; set; }
        private EnteredText EnteredText { get; set; }
        private IAppSettings m_AppSettings;
        private bool IsFeatureGivenStep { get; set; }

        private List<GherkinCodeCompletionWord> m_CompletionWords;
        private GherkinSimpleParser Parser;

        public GherkinCodeCompletionWordsProvider(TextDocument document, EnteredText enteredText, IAppSettings appSettings)
        {
            Document = document;
            EnteredText = enteredText;
            m_AppSettings = appSettings;
            Parser = new GherkinSimpleParser(document);
        }

        public List<GherkinCodeCompletionWord> CompletionWords
        {
            get
            {
                if (m_CompletionWords == null)
                {
                    m_CompletionWords = MakeCompletionWords();
                }

                return m_CompletionWords;
            }
        }

        private List<GherkinCodeCompletionWord> MakeCompletionWords()
        {
            List<GherkinCodeCompletionWord> completionWords = new List<GherkinCodeCompletionWord>();

            if (NeedCompletion())
            {
                GherkinDialect dialect = Parser.CurrentDialect;
                foreach (var keyword in GetGherkinKeywords(dialect))
                {
                    completionWords.Add(new GherkinCodeCompletionWord(keyword, m_AppSettings));
                }
            }

            return completionWords;
        }

        private bool NeedCompletion()
        {
            string filename = Document.FileName;

            return (IsLeadingTextOfCurrentLineWhiteSpace() &&
                    (filename != null) &&
                    filename.EndsWith(GherkinUtil.FEATURE_EXTENSION, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool IsLeadingTextOfCurrentLineWhiteSpace()
        {
            var line = Document.GetLineByNumber(EnteredText.Line);
            string text = GherkinFormatUtil.GetText(Document, line);
            string leading_text = text.Substring(0, EnteredText.Column - 1);

            return (leading_text.Trim().Length == 0);
        }

        private List<GherkinKeyword> GetGherkinKeywords(GherkinDialect dialect)
        {
            if (IsLeadingTextWhiteSpace())
            {
                List<GherkinKeyword> keywords = new List<GherkinKeyword>();
                keywords.Add(new GherkinKeyword("#language: ", TokenType.Language));
                AddRange(keywords, dialect.FeatureKeywords, TokenType.FeatureLine);
                return keywords;
            }
            else
            {
                return MakeGherkinKeywordsByLastToken();
            }
        }

        private void AddRange(List<GherkinKeyword> keywords, string[] texts, TokenType type)
        {
            foreach (var text in texts)
            {
                keywords.Add(new GherkinKeyword(text, type));
            }
        }

        private bool IsLeadingTextWhiteSpace()
        {
            for (int i = 1; i < EnteredText.Line - 1; i++)
            {
                string leading_text = GherkinFormatUtil.GetText(Document, Document.GetLineByNumber(i));
                if ((leading_text.Trim().Length > 0)) return false;
            }

            return IsLeadingTextOfCurrentLineWhiteSpace();
        }

        private List<GherkinKeyword> MakeGherkinKeywordsByLastToken()
        {
            List<GherkinKeyword> keywords = new List<GherkinKeyword>();
            Token token = GetLastEffectiveToken();
            GherkinDialect dialect = token.MatchedGherkinDialect;
            switch (token.MatchedType)
            {
                case TokenType.Language:
                    AddRange(keywords, dialect.FeatureKeywords, TokenType.FeatureLine);
                    break;
                case TokenType.FeatureLine:
                    AddRange(keywords, dialect.BackgroundKeywords, TokenType.BackgroundLine);
                    AddRange(keywords, dialect.ScenarioKeywords, TokenType.ScenarioLine);
                    AddRange(keywords, dialect.ScenarioOutlineKeywords, TokenType.ScenarioOutlineLine);
                    break;
                case TokenType.BackgroundLine:
                    AddRange(keywords, dialect.GivenStepKeywords, TokenType.StepLine);
                    break;
                case TokenType.ScenarioLine:
                case TokenType.ScenarioOutlineLine:
                    AddRange(keywords, dialect.GivenStepKeywords, TokenType.StepLine);
                    AddRange(keywords, dialect.WhenStepKeywords, TokenType.StepLine);
                    break;
                case TokenType.StepLine:
                    MakeCodeCompletion4Step(token, keywords);
                    break;
                case TokenType.ExamplesLine:
                    AddRange(keywords, dialect.ExamplesKeywords, TokenType.ExamplesLine);
                    AddRange(keywords, dialect.ScenarioKeywords, TokenType.ScenarioLine);
                    AddRange(keywords, dialect.ScenarioOutlineKeywords, TokenType.ScenarioOutlineLine);
                    break;

            }

            keywords.RemoveAll(x => x.Text.Contains("*"));  // remove all "*"

            return keywords;
        }

        private Token GetLastEffectiveToken()
        {
            if (EnteredText.Line > 1)
            {
                int lineNo = EnteredText.Line - 1;
                var document_line = Document.GetLineByNumber(lineNo);
                while (document_line != null)
                {
                    string text = GherkinFormatUtil.GetText(Document, document_line);
                    Token token = Parser.ParseToken(text);
                    switch (token.MatchedType)
                    {
                        case TokenType.Language:
                        case TokenType.FeatureLine:
                        case TokenType.BackgroundLine:
                        case TokenType.ScenarioLine:
                        case TokenType.ScenarioOutlineLine:
                        case TokenType.ExamplesLine:
                            return token;
                        case TokenType.StepLine:
                            if (IsGiven(token))
                            {
                                ConfirmIfGivenOfFeature(document_line.PreviousLine);
                                return token;
                            }
                            else if (IsWhen(token) || IsThen(token))
                            {
                                return token;
                            }
                            break;
                    }

                    document_line = document_line.PreviousLine;
                }
            }

            return Parser.ParseToken("");
        }

        private void MakeCodeCompletion4Step(Token lastEffectiveStep, List<GherkinKeyword> keywords)
        {
            GherkinDialect dialect = lastEffectiveStep.MatchedGherkinDialect;
            AddRange(keywords, dialect.AndStepKeywords, TokenType.StepLine);
            AddRange(keywords, dialect.ButStepKeywords, TokenType.StepLine);

            if (IsGiven(lastEffectiveStep) && IsFeatureGivenStep)
            {
                AddRange(keywords, dialect.WhenStepKeywords, TokenType.StepLine);
                AddRange(keywords, dialect.ThenStepKeywords, TokenType.StepLine);
            }
            else if (IsWhen(lastEffectiveStep))
            {
                AddRange(keywords, dialect.ThenStepKeywords, TokenType.StepLine);
            }
            else // It must be a then step
            {
                AddRange(keywords, dialect.ExamplesKeywords, TokenType.ExamplesLine);
                AddRange(keywords, dialect.ScenarioKeywords, TokenType.ScenarioLine);
                AddRange(keywords, dialect.ScenarioOutlineKeywords, TokenType.ScenarioOutlineLine);
            }
        }

        private void ConfirmIfGivenOfFeature(DocumentLine line)
        {
            var document_line = line;
            while (document_line != null)
            {
                string text = GherkinFormatUtil.GetText(Document, document_line);
                Token token = Parser.ParseToken(text);
                switch (token.MatchedType)
                {
                    case TokenType.BackgroundLine:
                        IsFeatureGivenStep = false;
                        return;
                    case TokenType.ScenarioLine:
                    case TokenType.ScenarioOutlineLine:
                        IsFeatureGivenStep = true;
                        return;
                }

                document_line = document_line.PreviousLine;
            }

            IsFeatureGivenStep = false;
        }

        private bool IsGiven(Token token)
        {
            return GherkinKeyword.IsStepKeyword(token.MatchedKeyword, token.MatchedGherkinDialect.GivenStepKeywords);
        }
        private bool IsWhen(Token token)
        {
            return GherkinKeyword.IsStepKeyword(token.MatchedKeyword, token.MatchedGherkinDialect.WhenStepKeywords);
        }
        private bool IsThen(Token token)
        {
            return GherkinKeyword.IsStepKeyword(token.MatchedKeyword, token.MatchedGherkinDialect.ThenStepKeywords);
        }
    }
}
