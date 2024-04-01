using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class IfThenElseToken : StatementToken
    {
        public IfThenElseToken(IToken parent, ParserToken source) : base(parent, source)
        {
        }

        public string VariableName { get; set; }

        public List<StatementToken> Then { get; internal set; }

        public List<StatementToken> Else { get; internal set; }

        public override IEnumerable<IToken> GetChildren()
        {
            return Then.Concat(Else);
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return Then
                .Concat(Then.SelectMany(t => t.GetDescentands()))
                .Concat(Else)
                .Concat(Else.SelectMany(e => e.GetDescentands()));
        }
    }
}