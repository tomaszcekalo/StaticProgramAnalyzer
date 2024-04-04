using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing.Predicates
{
    internal class IsVariablePredicate : IPredicate
    {
        public bool Evaluate(IToken token)
        {
            return token is VariableToken;
        }
    }
}
