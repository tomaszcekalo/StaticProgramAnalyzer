using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class CallToken : StatementToken, IHasProcedureName
    {
        public CallToken(IToken parent, ParserToken source) : base(parent, source)
        {
        }

        public string ProcedureName { get; set; }

        public override IEnumerable<IToken> GetChildren()
        {
            return new List<IToken>();
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return new List<IToken>();
        }
    }
}
