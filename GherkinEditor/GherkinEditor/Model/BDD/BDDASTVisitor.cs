using System.Collections.Generic;
using Gherkin.Ast;

namespace CucumberCpp
{
    public class BDDASTVisitor : IVisitable
    {
        BDDStepImplBuilderManager stepImplBuilder = new BDDStepImplBuilderManager();
        BDDFeatureBuilder featureBuilder = new BDDFeatureBuilder();

        public void BuildCPPTestCode(Feature feature)
        {
            feature.Visit(this);
        }

        public string StepImplFileName => BDDStepImplBuilderContext.StepImplementationFileName;
        public string FeatureFileName => featureBuilder.FeatureFileName;
        public string StepDefsImpl => stepImplBuilder.BuildStepImplCpp();
        public string FeatureImpl => featureBuilder.Build();

        // implementation of IVisitable interface

        void IVisitable.Accept(Feature feature)
        {
            stepImplBuilder.FeatureTitle = feature.Name;
            featureBuilder.FeatureTitle = feature.Name;
            featureBuilder.FeatureTags = feature.Tags;
        }

        void IVisitable.Accept(Background background)
        {
            featureBuilder.CreateBackgound();
        }

        void IVisitable.Accept(ScenarioOutline scenarioOutline)
        {
            featureBuilder.NewScenarioOutline(scenarioOutline);
        }

        void IVisitable.Accept(Examples examples)
        {
            featureBuilder.NewExamples(examples);
        }

        void IVisitable.Accept(DataTable dataTable)
        {
            stepImplBuilder.AddArg(dataTable);
        }

        void IVisitable.Accept(DocString docString)
        {
            stepImplBuilder.AddArg(docString);
        }

        void IVisitable.Accept(Step step)
        {
            BDDStepBuilder stepBuilder = stepImplBuilder.NewStep(step);
            featureBuilder.AddScenarioStep(stepBuilder);
        }

        void IVisitable.Accept(Scenario scenario)
        {
            featureBuilder.NewScenario(scenario);
        }

        void IVisitable.AcceptSenarioTags(IEnumerable<Tag> tags)
        {
            featureBuilder.ScenarioTags = tags;
        }

        /////// Not relevant functions of IVisitable //////

//        void IVisitable.Accept(Language language) { }
        void IVisitable.Accept(IEnumerable<Tag> tags) { }
    }
}
