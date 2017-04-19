using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Model;
using ICSharpCode.AvalonEdit.Folding;
using System.IO;
using ICSharpCode.AvalonEdit.Document;

namespace Gherkin.ViewModel
{
    class FoldingExecutor
    {
        private GherkinEditor MainGherkinEditor { get; set; }
        private GherkinEditor SubGherkinEditor { get; set; }
        private GherkinFoldingStrategy GherkinFoldingStrategy { get; set; }
        private XmlFoldingStrategy XmlFoldingStrategy { get; set; }
        private TextDocument Document => MainGherkinEditor.Document;

        public FoldingExecutor(GherkinEditor mainEditor, GherkinEditor subEditor)
        {
            MainGherkinEditor = mainEditor;
            SubGherkinEditor = subEditor;
        }

        public void InstallFoldingManager(string filePath)
        {
            if (HasFoldingStrategy(filePath))
            {
                MainGherkinEditor.InstallFoldingManager();
                SubGherkinEditor.InstallFoldingManager();
                CreateFoldingStrategy(filePath);
            }
            else
            {
                MainGherkinEditor.UnnstallFoldingManager();
                SubGherkinEditor.UnnstallFoldingManager();
                GherkinFoldingStrategy = null;
                XmlFoldingStrategy = null;
            }
        }

        public List<NewFolding> UpdateFeatureFoldings(bool isCloseTablesFolding, bool isCloseScenarioFolding, bool refresh)
        {
            if (!IsCurrentFoldingFeature) return new List<NewFolding>();

            List<FoldingManager> managers = new List<FoldingManager>();
            if (MainGherkinEditor.FoldingManager != null)
                managers.Add(MainGherkinEditor.FoldingManager);
            if (SubGherkinEditor.FoldingManager != null)
                managers.Add(SubGherkinEditor.FoldingManager);

            return GherkinFoldingStrategy.UpdateFoldings(managers, Document, isCloseTablesFolding, isCloseScenarioFolding, refresh);
        }

        public void UpdateXMLFoldings()
        {
            if (!IsCurrentFoldingXML) return;

            List<FoldingManager> managers = new List<FoldingManager>();
            if (MainGherkinEditor.FoldingManager != null)
                managers.Add(MainGherkinEditor.FoldingManager);
            if (SubGherkinEditor.FoldingManager != null)
                managers.Add(SubGherkinEditor.FoldingManager);

            foreach (var manager in managers)
            {
                XmlFoldingStrategy.UpdateFoldings(manager, Document);
            }
        }

        public bool IsCurrentFoldingFeature => (GherkinFoldingStrategy != null);
        public bool IsCurrentFoldingXML => (XmlFoldingStrategy != null);

        private bool HasFoldingStrategy(string filePath)
        {
            return GherkinUtil.IsFeatureFile(filePath) || IsXMLFile(filePath);
        }

        private bool IsXMLFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            var ext = Path.GetExtension(filePath);
            return GherkinUtil.HasExtension(filePath, ".xml") ||
                   GherkinUtil.HasExtension(filePath, ".xaml") ||
                   GherkinUtil.HasExtension(filePath, ".config");
        }

        private void CreateFoldingStrategy(string filePath)
        {
            GherkinFoldingStrategy = null;
            XmlFoldingStrategy = null;

            if (GherkinUtil.IsFeatureFile(filePath))
                GherkinFoldingStrategy = new GherkinFoldingStrategy();
            else if (IsXMLFile(filePath))
                XmlFoldingStrategy = new XmlFoldingStrategy();
        }
    }
}
