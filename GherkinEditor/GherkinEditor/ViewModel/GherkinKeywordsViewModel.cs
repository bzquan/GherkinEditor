using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Model;
using System.Globalization;

namespace Gherkin.ViewModel
{
    public class GherkinKeywordsViewModel : NotifyPropertyChangedBase
    {
        private static Lazy<GherkinDialectProviderExtention> s_GherkinDialectcs = new Lazy<GherkinDialectProviderExtention>(() => new GherkinDialectProviderExtention());

        private Util.Languages m_AppLanguage;
        private List<GherkinKeywordsCurrent> m_GherkinLanguages;
        private ObservableCollection<GherkinKeywordsCurrent> m_ObservableGherkinLanguages;
        private int m_CurrentLanguageIndex;

        public GherkinKeywordsViewModel(Util.IAppSettings appSettings)
        {
            m_AppLanguage = appSettings.Language;
        }

        public ObservableCollection<GherkinKeywordsCurrent> GherkinLanguages
        {
            get
            {
                if (m_GherkinLanguages == null)
                {
                    m_GherkinLanguages = LoadGherkinKeywords();
                    m_ObservableGherkinLanguages = new ObservableCollection<GherkinKeywordsCurrent>(m_GherkinLanguages);
                    SetDefaultLanguage();
                }

                return m_ObservableGherkinLanguages;
            }
        }

        public string KeywordsHtml
        {
            get { return SelectedItem.KeywordsHtml; }
        }

        public GherkinKeywordsCurrent SelectedItem => GherkinLanguages[m_CurrentLanguageIndex];

        public int SelectedLanguageIndex
        {
            get { return m_CurrentLanguageIndex; }
            set
            {
                m_CurrentLanguageIndex = value;
                base.OnPropertyChanged(nameof(KeywordsHtml));
            }
        }

        private List<GherkinKeywordsCurrent> LoadGherkinKeywords()
        {
            List<GherkinKeywordsCurrent> gherkinKeywordsList = new List<GherkinKeywordsCurrent>();
            GherkinLanguage[] languages = s_GherkinDialectcs.Value.GherkinLanguages;

            Array.Sort(languages, delegate (GherkinLanguage lang1, GherkinLanguage lang2)
            {
                return lang1.name.CompareTo(lang2.name);
            });

            StringBuilder sb = new StringBuilder();
            foreach (var lang in languages)
            {
                gherkinKeywordsList.Add(new GherkinKeywordsCurrent(lang));
            }

            return gherkinKeywordsList;
        }

        private void SetDefaultLanguage()
        {
            string language_key = GetLanguageKey();
            int index = IndexOfGherkinKeywords(language_key);
            if (index < 0)
            {
                index = IndexOfGherkinKeywords("en");
            }
            SelectedLanguageIndex = index;
        }

        private string GetLanguageKey()
        {
            switch (m_AppLanguage)
            {
                case Util.Languages.Chinese:
                    return "zh-CN";
                case Util.Languages.Japanese:
                    return "ja";
                default:
                    CultureInfo ci = CultureInfo.InstalledUICulture;
                    return ci.TwoLetterISOLanguageName;
            }
        }

        private int IndexOfGherkinKeywords(string language_key) =>
            m_GherkinLanguages.FindIndex(x => x.IsSame(language_key));
    }
}
