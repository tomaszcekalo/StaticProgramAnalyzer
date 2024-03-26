using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing.Predicates
{
    public class IsIfTheElsePredicate : IPredicate
    {
        public bool Evaluate(IToken token)
        {
            return token is IfThenElseToken;
        }
    }
}
