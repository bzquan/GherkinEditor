using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gherkin.Ast;

namespace CucumberCpp
{
    public abstract class BDDAbstrctScenarioBuilder
    {
        private List<BDDStepBuilder> m_StepBuilderList = new List<BDDStepBuilder>();

        public string FeatureClassName { get; set; }
        public IEnumerable<Tag> ScenarioTags { get; set; }

        public void AddScenarioStep(BDDStepBuilder stepBuilder)
        {
            if (stepBuilder.HasMockAttribute())
            {
                InsertAtBegining(stepBuilder);
            }
            else
            {
                m_StepBuilderList.Add(stepBuilder);
            }
        }

        private void InsertAtBegining(BDDStepBuilder stepBuilder)
        {
            int index = m_StepBuilderList.FindLastIndex(x => x.HasMockAttribute());
            if (index != -1)
                m_StepBuilderList.Insert(index + 1, stepBuilder);
            else
                m_StepBuilderList.Insert(0, stepBuilder);
        }

        public string BuildScenario()
        {
            MakeTableAndDocStringSeqNo();
            return BuildScenarioImpl();
        }

        public abstract string BuildScenarioImpl();

        protected string BuildGUIDTag()
        {
            if (ScenarioTags != null)
            {
                Tag guidTag = ScenarioTags.FirstOrDefault(tag => tag.IsGUID());
                if (guidTag != null)
                {
                    return "Spec(\"" + guidTag.Name + "\");";
                }
            }

            return "Spec(\"GUID has not been defined!\");";
        }

        protected string BuildSteps(string indent)
        {
            StringBuilder stepOfScenario = new StringBuilder();
            foreach (BDDStepBuilder stepBuilder in m_StepBuilderList)
            {
                stepOfScenario.AppendLine(stepBuilder.BuildStepForScenario(indent));
            }

            return stepOfScenario.ToString();
        }

        private void MakeTableAndDocStringSeqNo()
        {
            int tableSeqNo = 0;
            int docStringSeqNo = 0;
            foreach(BDDStepBuilder stepBuilder in m_StepBuilderList)
            {
                if (stepBuilder.TableArg != null)
                {
                    stepBuilder.TableSeqNo = tableSeqNo++;
                }
                if (stepBuilder.DocStringArg != null)
                {
                    stepBuilder.DocStringSeqNo = docStringSeqNo++;
                }
            }
        }
    }
}
