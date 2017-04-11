namespace Brainfuck
{
    public interface IBrainfuckInterpreter
    {
        void Parse(string input);
        void Execute();
        string ToCStatements();
        string ToBrainfuckStatement(int lineLength = 0);
        string ToIntermediaryRepresentation();
    }
}
