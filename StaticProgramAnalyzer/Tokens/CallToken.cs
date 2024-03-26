using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class CallToken : StatementToken
    {
        public CallToken(IToken parent) : base(parent)
        {
        }

        public string ProcedureName { get; internal set; }

        public override IEnumerable<IToken> GetChildren()
        {
            return new List<IToken>();
        }
    }
}
