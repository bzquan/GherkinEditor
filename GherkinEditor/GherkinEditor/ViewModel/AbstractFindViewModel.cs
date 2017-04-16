using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gherkin.Util;

namespace Gherkin.ViewModel
{
    public abstract class AbstractFindViewModel : NotifyPropertyChangedBase
    {
        protected IAppSettings m_AppSettings;
        protected SearchCondition m_SearchCondition = new SearchCondition();

        public AbstractFindViewModel(IAppSettings appSettings)
        {
            m_AppSettings = appSettings;
        }

        public string TextToFind
        {
            get { return m_SearchCondition.TextToFind; }
            set
            {
                m_SearchCondition.TextToFind = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsCaseSensitive
        {
            get { return m_SearchCondition.IsCaseSensitive; }
            set
            {
                m_SearchCondition.IsCaseSensitive = value;
                m_AppSettings.IsCaseSensitiveInFind = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsMatchWholeWord
        {
            get { return m_SearchCondition.IsMatchWholeWord; }
            set
            {
                m_SearchCondition.IsMatchWholeWord = value;
                m_AppSettings.IsMatchWholeWordInFind = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsUseRegex
        {
            get { return m_SearchCondition.IsUseRegex; }
            set
            {
                m_SearchCondition.IsUseRegex = value;
                m_AppSettings.IsUseRegexInFind = value;
                base.OnPropertyChanged();
            }
        }

        public bool IsUseWildcards
        {
            get { return m_SearchCondition.IsUseWildcards; }
            set
            {
                m_SearchCondition.IsUseWildcards = value;
                m_AppSettings.IsUseWildcardsInFind = value;
                base.OnPropertyChanged();
            }
        }
    }
}
