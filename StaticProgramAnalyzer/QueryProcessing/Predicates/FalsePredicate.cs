using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing.Predicates
{
    internal class FalsePredicate : IPredicate
    {
        public bool Evaluate(IToken token)
        {
            return false;
        }
    }
}
