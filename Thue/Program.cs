
namespace Thue
{
    class Program
    {
        private static string HelloWorld = "a::=~Hello World!" + System.Environment.NewLine +
                                           "::=" + System.Environment.NewLine +
                                           "a" + System.Environment.NewLine;

        private static string IncrementBinary = "1_::=1++" + System.Environment.NewLine +
                                                "0_::=1" + System.Environment.NewLine +
                                                "01++::=10" + System.Environment.NewLine +
                                                "11++::=1++0" + System.Environment.NewLine +
                                                "_0::=_" + System.Environment.NewLine +
                                                "_1++::=10" + System.Environment.NewLine +
                                                "::=" + System.Environment.NewLine +
                                                "_1111111111_" + System.Environment.NewLine;

        // Works only with LeftToRight and Ascending
        private static string Roman = "*::=I" + System.Environment.NewLine +
                                      "IIIII::=V" + System.Environment.NewLine +
                                      "IIII::=IV" + System.Environment.NewLine +
                                      "VV::=X" + System.Environment.NewLine +
                                      "VIV::=IX" + System.Environment.NewLine +
                                      "XXXXX::=L" + System.Environment.NewLine +
                                      "XXXX::=XL" + System.Environment.NewLine +
                                      "LL::=C" + System.Environment.NewLine +
                                      "LXL::=XC" + System.Environment.NewLine +
                                      "CCCCC::=D" + System.Environment.NewLine +
                                      "CCCC::=CD" + System.Environment.NewLine +
                                      "DD::=M" + System.Environment.NewLine +
                                      "DCD::=CM" + System.Environment.NewLine +
                                      "::=" + System.Environment.NewLine +
                                      "*************************************************************************************";

        static void Main(string[] args)
        {
            ThueInterpreter interpreter = new ThueInterpreter(() => string.Empty, System.Console.Write, ThueInterpreter.TokensProcessingOrders.LeftToRight, ThueInterpreter.RuleSelectionPolicies.Ascending);
            interpreter.Parse(Roman);
            interpreter.Execute();
        }
    }
}
