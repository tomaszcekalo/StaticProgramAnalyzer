using StaticProgramAnalyzer.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.QueryProcessing.Predicates
{
    public interface IPredicate
    {
        public bool Evaluate(IToken token);
    }
}
