using StaticProgramAnalyzer.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace StaticProgramAnalyzer.Tokens
{
    public class WhileToken : StatementToken, IUseVariableToken
    {
        public WhileToken(IToken parent, ParserToken source, int statementNumber) : base(parent, source, statementNumber)
        {
        }

        public string VariableName { get; set; }
        public List<StatementToken> StatementList { get; set; }

        public override IEnumerable<IToken> GetChildren()
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
