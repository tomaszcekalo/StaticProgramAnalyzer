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

        public bool FollowsStar(StatementToken left, StatementToken right)
        {
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
