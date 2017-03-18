namespace Gherkin.Ast
{
    public abstract class StepArgument : IVisit
    {
        public abstract void Visit(IVisitable visitable);
    }
}