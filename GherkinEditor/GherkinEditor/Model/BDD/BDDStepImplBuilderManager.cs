using Gherkin.Ast;

namespace CucumberCpp
{
    class BDDStepImplBuilderManager : BDDAbstractBuilder
    {
        BDDStepImplCppBuilder stepImplCppBuilder = new BDDStepImplCppBuilder();
        BDDStepBuilder currentStep;

        public override string FeatureTitle
        {
            get { return base.FeatureTitle; }
            set
            {
                base.FeatureTitle = value;
                BDDStepImplBuilderContext.FeatureTitle = value;
            }
        }

        public BDDStepBuilder NewStep(Step step)
        {
            currentStep = BDDStepImplBuilderContext.NewStep(step);
            return currentStep;
        }

        public void AddArg(DataTable dataTable)
        {
            if (currentStep != null)
            {
                currentStep.TableArg = dataTable;
            }
        }

        public void AddArg(DocString docString)
        {
            if (currentStep != null)
            {
                currentStep.DocStringArg = docString;
            }
        }

        public string BuildStepImplCpp()
        {
            return stepImplCppBuilder.BuildStepImplCpp();
        }
    }
}
