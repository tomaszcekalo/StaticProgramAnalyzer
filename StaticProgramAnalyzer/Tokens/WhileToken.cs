using StaticProgramAnalyzer.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace StaticProgramAnalyzer.Tokens
{
    public class WhileToken : StatementToken, IUseVariableToken, IDeterminesFollows
    {
        public WhileToken(IToken parent, ParserToken source, int statementNumber) : base(parent, source, statementNumber)
        {
        }

        public string VariableName { get; set; }
        public List<StatementToken> StatementList { get; set; }

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
            return StatementList.IndexOf(left) < StatementList.IndexOf(right);
        }

        public override IEnumerable<StatementToken> GetChildren()
        {
            return StatementList;
        }

        public override IEnumerable<IToken> GetDescentands()
        {
            return StatementList.Concat(
                StatementList.SelectMany(x => x.GetDescentands()));
        }
    }
}