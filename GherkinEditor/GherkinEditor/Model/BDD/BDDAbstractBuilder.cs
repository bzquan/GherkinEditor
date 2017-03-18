using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gherkin.Ast;

namespace CucumberCpp
{
    public class BDDAbstractBuilder
    {
        private string m_FeatureTitle;

        public virtual string FeatureTitle
        {
            get { return m_FeatureTitle; }
            set { m_FeatureTitle = BDDUtil.MakeIdentifier(value); }
        }
    }
}
