using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IEsolang;

namespace Thue
{
    public class ThueInterpreter : IInterpreter
    {
        public enum TokensProcessingOrders
        {
            Random,
            LeftToRight,
            RightToLeft
        }

        public enum RuleSelectionPolicies
        {
            Random,
            Ascending,
            Descending
        }

        private class Rule
        {
            public string Original { get; set; }
            public string Replacement { get; set; }
        }

        private Func<string> InputFunc { get; }
        private Action<string> OutputAction { get; }
        private TokensProcessingOrders TokensProcessingOrder { get; }
        private RuleSelectionPolicies RuleSelectionPolicy { get; }

        private Random Rng { get; } = new Random();
        private List<Rule> Rules { get; } = new List<Rule>();

        private string InitialState { get; set; }

        public ThueInterpreter(Func<string> inputFunc, Action<string> outputAction, TokensProcessingOrders tokensProcessingOrder, RuleSelectionPolicies ruleSelectionPolicy)
        {
            InputFunc = inputFunc;
            OutputAction = outputAction;
            TokensProcessingOrder = tokensProcessingOrder;
            RuleSelectionPolicy = ruleSelectionPolicy;
        }

        public void Parse(string input)
        {
            StringBuilder initialState = new StringBuilder();
            string[] lines = input.Split('\r', '\n');
            bool rulesParsed = false;
            foreach (string line in lines)
            {
                int equalsIndex = line.IndexOf("::=", StringComparison.InvariantCultureIgnoreCase);
                if (equalsIndex < 0)
                {
                    if (rulesParsed)
                        initialState.Append(line);
                }
                else if (equalsIndex == 0)
                    rulesParsed = true;
                else
                {
                    string original = line.Substring(0, equalsIndex);
                    string replacement = line.Substring(equalsIndex + 3);
                    Rules.Add(new Rule { Original = original, Replacement = replacement.Trim()});
                }
            }

            InitialState = initialState.ToString();
        }

        public void Execute()
        {
            StringBuilder data = new StringBuilder(InitialState);
            while (true)
            {
                bool stop = Step(data);
                if (stop)
                {
                    Debug.Print("DATA: "+data);
                    break;
                }
            }
        }

        private bool Step(StringBuilder data)
        {
            Rule rule = FindRule(data);
            if (rule == null)
                return true;

            int tokenStart = -1;
            switch (TokensProcessingOrder)
            {
                case TokensProcessingOrders.LeftToRight:
                    tokenStart = data.ToString().IndexOf(rule.Original, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case TokensProcessingOrders.RightToLeft:
                    tokenStart = data.ToString().LastIndexOf(rule.Original, StringComparison.InvariantCultureIgnoreCase);
                    break;
                case TokensProcessingOrders.Random:
                {
                    List<int> tokenStartCandidates = new List<int>();
                    string s = data.ToString();
                    int index = -1;
                    while (true)
                    {
                        index = s.IndexOf(rule.Original, index+1, StringComparison.InvariantCultureIgnoreCase);
                        if (index < 0)
                            break;
                        tokenStartCandidates.Add(index);
                    }
                    tokenStart = tokenStartCandidates[Rng.Next(tokenStartCandidates.Count)];
                    break;
                }
            }

            ApplyRule(rule, tokenStart, rule.Original.Length, data);

            return false;
        }

        private Rule FindRule(StringBuilder data)
        {
            switch (RuleSelectionPolicy)
            {
                case RuleSelectionPolicies.Ascending:
                    foreach (Rule rule in Rules)
                    {
                        if (data.ToString().IndexOf(rule.Original, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return rule;
                    }

                    break;
                case RuleSelectionPolicies.Descending:
                    foreach (Rule rule in Rules.AsEnumerable().Reverse())
                    {
                        if (data.ToString().IndexOf(rule.Original, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            return rule;
                    }
                    break;
                case RuleSelectionPolicies.Random:
                {
                    List<Rule> matchingRules = new List<Rule>();
                    foreach(Rule rule in Rules)
                        if (data.ToString().IndexOf(rule.Original, StringComparison.InvariantCultureIgnoreCase) >= 0)
                            matchingRules.Add(rule);
                    if (matchingRules.Count > 0)
                        return matchingRules[Rng.Next(matchingRules.Count)];
                    break;
                }
            }

            return null;
        }

        private void ApplyRule(Rule rule, int tokenStart, int length, StringBuilder data)
        {
            data.Remove(tokenStart, length);
            string replacement = rule.Replacement;
            // Input
            while (true)
            {
                int index = replacement.IndexOf(":::", StringComparison.InvariantCultureIgnoreCase);
                if (index == -1)
                    break;
                string input = InputFunc();
                data.Remove(index, 3);
                data.Insert(index, input);
            }
            // Output
            if (replacement.StartsWith("~"))
            {
                string output = replacement.Substring(1);
                OutputAction(output);
            }
            // Apply replacement
            else
            {
                data.Insert(tokenStart, replacement);
            }
        }
    }
}
