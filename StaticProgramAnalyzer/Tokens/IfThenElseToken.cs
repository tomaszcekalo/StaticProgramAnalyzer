using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    internal class IfThenElseToken : StatementToken, IUseVariableToken, IDeterminesFollows
    {
        public IfThenElseToken(IToken parent, ParserToken source, int statementNumber) : base(parent, source, statementNumber)
        {
        }

        public string VariableName { get; set; }

        public List<StatementToken> Then { get; internal set; }

        public List<StatementToken> Else { get; internal set; }

        public bool Follows(StatementToken left, StatementToken right)
        {
            if(Then.Contains(left) && Then.Contains(right))
                return Then.IndexOf(left) == Then.IndexOf(right) - 1;
            if(Else.Contains(left) && Else.Contains(right))
                return Else.IndexOf(left) == Else.IndexOf(right) - 1;
            return false;
        }

        public bool FollowsStar(StatementToken left, StatementToken right)
        {
            if (Then.Contains(left) && Then.Contains(right))
                return Then.IndexOf(left) < Then.IndexOf(right);
            if (Else.Contains(left) && Else.Contains(right))
                return Else.IndexOf(left) < Else.IndexOf(right);
            return false;
        }

        public override IEnumerable<StatementToken> GetChildren()
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