namespace IEsolang
{
    public interface IInterpreter
    {
        void Parse(string input);
        void Execute();
    }
}
