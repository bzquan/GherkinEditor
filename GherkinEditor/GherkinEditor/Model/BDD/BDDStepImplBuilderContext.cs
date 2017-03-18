using System.Collections.Generic;
using Gherkin.Ast;
using Gherkin.Model;
using System.IO;

namespace CucumberCpp
{
    class BDDStepImplBuilderContext
    {
        static List<BDDStepBuilder> StepBuilders = new List<BDDStepBuilder>();
        public static string FeatureTitle { get; set; }
        public static string StepImplementationFileName => BDDUtil.MakeIdentifier(FeatureTitle) + "_Steps.cpp";
        public static List<BDDStepBuilder> _NonDuplicatedStepBuilders = new List<BDDStepBuilder>();
        public static Gherkin.GherkinDialect GherkinDialect { get; set; }

        public static void StartBuildFeature(Feature feature)
        {
            StepBuilders.Clear();
            _NonDuplicatedStepBuilders.Clear();
            FeatureTitle = "";

            GherkinDialectProviderExtention dialectProvider = new GherkinDialectProviderExtention();
            GherkinDialect = dialectProvider.GetCurrentDialect(feature.Language);
        }

        public static BDDStepBuilder NewStep(Step step)
        {
            BDDStepBuilder stepBuilder = new BDDStepBuilder(step);
            StepBuilders.Add(stepBuilder);
            return stepBuilder;
        }

        public static List<BDDStepBuilder> NonDuplicatedStepBuilders
        {
            get
            {
                if (_NonDuplicatedStepBuilders.Count == 0)
                {
                    MakeNonDuplicatedStepBuilders();
                }
                return _NonDuplicatedStepBuilders;
            }
        }

        private static void MakeNonDuplicatedStepBuilders()
        {
            foreach (BDDStepBuilder stepBuilder in StepBuilders)
            {
                BDDStepBuilder sameStepBuilder = _NonDuplicatedStepBuilders.Find(x => x.StepImpSkeleton == stepBuilder.StepImpSkeleton);
                if (sameStepBuilder == null)
                {
                    _NonDuplicatedStepBuilders.Add(stepBuilder);
                    sameStepBuilder = stepBuilder;
                }

                sameStepBuilder.AddStepComments(stepBuilder.StepText);
            }
        }
    }
}
