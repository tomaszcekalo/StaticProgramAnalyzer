using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing.Predicates
{
    public class IsConstantPredicate : IPredicate
    {
        public bool Evaluate(IToken token)
        {
            return token is ConstantToken;
        }
    }
}
