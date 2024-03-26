using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing.Predicates
{
    public class IsWhilePredicate : IPredicate
    {
        public bool Evaluate(IToken token)
        {
            return token is WhileToken;
        }
    }
}
