using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing.Predicates
{
    public class IsProcedurePredicate : IPredicate
    {
        public bool Evaluate(IToken token)
        {
            return token is ProcedureToken;
        }
    }
}
