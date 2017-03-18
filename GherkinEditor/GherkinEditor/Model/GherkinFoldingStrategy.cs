using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using Gherkin.Util;

namespace Gherkin.Model
{
	public class GherkinFoldingStrategy
    {
        private List<NewFolding> m_LastFolding = new List<NewFolding>();

        public List<NewFolding> UpdateFoldings(List<FoldingManager> managers, TextDocument document, bool isCloseTablesFolding, bool isCloseStepsFolding, bool refresh)
		{
            if (managers.Count == 0) return GetScenarioFoldings();

            List<NewFolding> newFoldings = CreateNewFoldings(document, isCloseTablesFolding, isCloseStepsFolding);

            foreach (FoldingManager manager in managers)
            {
                if (refresh)
                {
                    manager.Clear();
                }
                manager.UpdateFoldings(newFoldings, firstErrorOffset: -1);
            }

            m_LastFolding = newFoldings;
            return GetScenarioFoldings();
        }

        private List<NewFolding> GetScenarioFoldings()
        {
            List<NewFolding> scenarioFoldings = new List<NewFolding>();
            foreach (var folding in m_LastFolding)
            {
                if (folding.IsDefinition) scenarioFoldings.Add(folding);
            }

            return scenarioFoldings;
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// Following folding types are supported 
        /// Background, Scenario, ScenarioOutline, Examples, Table
        /// </summary>
        private List<NewFolding> CreateNewFoldings(TextDocument document, bool isCloseTablesFolding, bool isCloseStepsFolding)
        {
            GherkinFoldingBuilder builder = new GherkinFoldingBuilder(document, isCloseTablesFolding, isCloseStepsFolding);
            return builder.BuildFoldings();
        }
	}
}
