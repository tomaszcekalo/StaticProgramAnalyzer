using StaticProgramAnalyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StaticProgramAnalyzer.Tokens
{
    public class ProcedureToken : IHasProcedureName, IDeterminesFollows
    {
        public string ProcedureName { get; set; }
        public List<StatementToken> StatementList { get; set; }
        public ParserToken Source { get; set; }
        public IToken Parent { get => null; set { } }

        public bool Follows(StatementToken left, StatementToken right)
        {
            return StatementList.IndexOf(left) == StatementList.IndexOf(right) - 1;
        }

        public bool Follows(int statementNumber, StatementToken right)
        {
            var specifiedStatement = StatementList.Find(x => x.StatementNumber == statementNumber);
            if (specifiedStatement == null)
                return false;
            return Follows(specifiedStatement, right);
        }

        public bool Follows(StatementToken left, int statementNumber)
        {
            var specifiedStatement = StatementList.Find(x => x.StatementNumber == statementNumber);
            if (specifiedStatement == null)
                return false;
            return Follows(left, specifiedStatement);
        }

        public bool Follows(int leftStatementNumber, int rightStatementNumber)
        {
            var left = StatementList.Find(x => x.StatementNumber == leftStatementNumber);
            var right = StatementList.Find(x => x.StatementNumber == rightStatementNumber);
            if (left == null || right == null)
                return false;
            return Follows(left, right);
        }

        public bool FollowsStar(StatementToken left, StatementToken right)
        {
            return StatementList.IndexOf(left) < StatementList.IndexOf(right);
        }

        public bool FollowsStar(int statementNumber, StatementToken right)
        {
            var left = StatementList.Find(x => x.StatementNumber == statementNumber);
            if (left != null)
            {
                return FollowsStar(left, right);
            }
            return false;
        }

        public bool FollowsStar(StatementToken left, int statementNumber)
        {
            var right = StatementList.Find(x => x.StatementNumber == statementNumber);
            if (right != null)
            {
                return FollowsStar(left, right);
            }
            return false;
        }

        public bool FollowsStar(int leftStatementNumber, int rightStatementNumber)
        {
            var left = StatementList.Find(x => x.StatementNumber == leftStatementNumber);
            var right = StatementList.Find(x => x.StatementNumber == rightStatementNumber);
            if (left is null || right is null)
            {
                return false;
            }
            return FollowsStar(left, right);
        }

        public IEnumerable<StatementToken> GetChildren()
        {
            return StatementList;
        }

        public IEnumerable<IToken> GetDescentands()
        {
            return StatementList.Concat(
                StatementList.SelectMany(x => x.GetDescentands()));
        }

        public override string ToString()
        {
            return ProcedureName;
        }
    }
}