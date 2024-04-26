using System;
using System.Collections.Generic;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public interface IDeterminesFollows : IToken
    {
        public bool Follows(StatementToken left, StatementToken right);
        public bool Follows(int statementNumber, StatementToken right);
        public bool Follows(StatementToken left, int statementNumber);
        public bool Follows(int leftStatementNumber, int rightStatementNumber);
        public bool FollowsStar(StatementToken left, StatementToken right);
        public bool FollowsStar(int statementNumber, StatementToken right);
        public bool FollowsStar(StatementToken left, int statementNumber);
        public bool FollowsStar(int leftStatementNumber, int rightStatementNumber);
    }
}
