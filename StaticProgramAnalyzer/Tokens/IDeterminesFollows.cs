using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public interface IDeterminesFollows : IToken
    {
        public bool Follows(StatementToken left, StatementToken right);
        public bool FollowsStar(StatementToken left, StatementToken right);
    }
}
