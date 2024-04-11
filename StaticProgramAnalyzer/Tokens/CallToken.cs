using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class CallToken : StatementToken, IHasProcedureName
    {
        public CallToken(IToken parent, ParserToken source, int statementNumber) : base(parent, source, statementNumber)
        {
        }

        public string ProcedureName { get; set; }

        public override IEnumerable<StatementToken> GetChildren()
        {
            return null;
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return new List<IToken>();
        }
    }
}
